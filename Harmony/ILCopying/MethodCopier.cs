using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace HarmonyErdelf.ILCopying
{
	public class MethodCopier
	{
		readonly MethodBodyReader reader;
		readonly List<MethodInfo> transpilers = new List<MethodInfo>();

		public MethodCopier(MethodBase fromMethod, DynamicMethod toDynamicMethod, LocalBuilder[] existingVariables = null)
		{
			if (fromMethod == null) throw new Exception("method cannot be null");
            ILGenerator generator = toDynamicMethod.GetILGenerator();
            this.reader = new MethodBodyReader(fromMethod, generator);
            this.reader.DeclareVariables(existingVariables);
            this.reader.ReadInstructions();
		}

        public void AddTranspiler(MethodInfo transpiler) => this.transpilers.Add(transpiler);

        public void Emit(Label endLabel) => this.reader.FinalizeILCodes(this.transpilers, endLabel);
    }

	public class MethodBodyReader
	{
		readonly ILGenerator generator;

		readonly MethodBase method;
		readonly Module module;
		readonly Type[] typeArguments;
		readonly Type[] methodArguments;
		readonly ByteBuffer ilBytes;
		readonly ParameterInfo this_parameter;
		readonly ParameterInfo[] parameters;
		readonly IList<LocalVariableInfo> locals;
		readonly IList<ExceptionHandlingClause> exceptions;
		List<ILInstruction> ilInstructions;

		LocalBuilder[] variables;

		public static List<ILInstruction> GetInstructions(MethodBase method)
		{
			if (method == null) throw new Exception("method cannot be null");
            MethodBodyReader reader = new MethodBodyReader(method, null);
			reader.ReadInstructions();
			return reader.ilInstructions;
		}

		// constructor

		public MethodBodyReader(MethodBase method, ILGenerator generator)
		{
			this.generator = generator;
			this.method = method;
            this.module = method.Module;

            MethodBody body = method.GetMethodBody();
			if (body == null)
				throw new ArgumentException("Method has no body");

            byte[] bytes = body.GetILAsByteArray();
			if (bytes == null)
				throw new ArgumentException("Can not get the bytes of the method");
            this.ilBytes = new ByteBuffer(bytes);
            this.ilInstructions = new List<ILInstruction>((bytes.Length + 1) / 2);

			Type type = method.DeclaringType;
			if (type != null)
			{
				if (type.IsGenericType || type.IsGenericTypeDefinition)
                    this.typeArguments = type.GetGenericArguments();
			}
			if (method.IsGenericMethod || method.IsGenericMethodDefinition)
                this.methodArguments = method.GetGenericArguments();

			if (!method.IsStatic)
                this.this_parameter = new ThisParameter(method);
            this.parameters = method.GetParameters();

            this.locals = body.LocalVariables;
            this.exceptions = body.ExceptionHandlingClauses;
		}

		// read and parse IL codes

		public void ReadInstructions()
		{
			while (this.ilBytes.position < this.ilBytes.buffer.Length)
			{
                int loc = this.ilBytes.position; // get location first (ReadOpCode will advance it)
                ILInstruction instruction = new ILInstruction(ReadOpCode())
                {
                    offset = loc
                };
                ReadOperand(instruction);
                this.ilInstructions.Add(instruction);
			}

			ResolveBranches();
		}

		void ResolveBranches()
		{
			foreach (ILInstruction instruction in this.ilInstructions)
			{
				switch (instruction.opcode.OperandType)
				{
					case OperandType.ShortInlineBrTarget:
					case OperandType.InlineBrTarget:
						instruction.operand = GetInstruction((int)instruction.operand);
						break;

					case OperandType.InlineSwitch:
                        int[] offsets = (int[])instruction.operand;
                        ILInstruction[] branches = new ILInstruction[offsets.Length];
						for (int j = 0; j < offsets.Length; j++)
							branches[j] = GetInstruction(offsets[j]);

						instruction.operand = branches;
						break;
				}
			}
		}

		// declare local variables
		public void DeclareVariables(LocalBuilder[] existingVariables)
		{
			if (this.generator == null) return;
			if (existingVariables != null)
                this.variables = existingVariables;
			else
                this.variables = this.locals.Select(
					lvi => this.generator.DeclareLocal(lvi.LocalType, lvi.IsPinned)
				).ToArray();
		}

		// use parsed IL codes and emit them to a generator

		public void FinalizeILCodes(List<MethodInfo> transpilers, Label endLabel)
		{
			if (this.generator == null) return;

            // pass1 - define labels and add them to instructions that are target of a jump
            this.ilInstructions.ForEach(ilInstruction =>
			{
				switch (ilInstruction.opcode.OperandType)
				{
					case OperandType.InlineSwitch:
						{
                            if (ilInstruction.operand is ILInstruction[] targets)
                            {
                                List<Label> labels = new List<Label>();
                                foreach (ILInstruction target in targets)
                                {
                                    Label label = this.generator.DefineLabel();
                                    target.labels.Add(label);
                                    labels.Add(label);
                                }
                                ilInstruction.argument = labels.ToArray();
                            }
                            break;
						}

					case OperandType.ShortInlineBrTarget:
					case OperandType.InlineBrTarget:
						{
                            if (ilInstruction.operand is ILInstruction target)
                            {
                                Label label = this.generator.DefineLabel();
                                target.labels.Add(label);
                                ilInstruction.argument = label;
                            }
                            break;
						}
				}
			});

            // pass2 - filter through all processors
            CodeTranspiler codeTranspiler = new CodeTranspiler(this.ilInstructions);
			transpilers.ForEach(transpiler => codeTranspiler.Add(transpiler));
            IEnumerable<CodeInstruction> codeInstructions = codeTranspiler.GetResult(this.generator, this.method)
				.Select(instruction =>
				{
					// TODO - improve the logic here. for now, we replace all short jumps
					//        with long jumps regardless of how far the jump is
					//
					new Dictionary<OpCode, OpCode>
					{
						{ OpCodes.Beq_S, OpCodes.Beq },
						{ OpCodes.Bge_S, OpCodes.Bge },
						{ OpCodes.Bge_Un_S, OpCodes.Bge_Un },
						{ OpCodes.Bgt_S, OpCodes.Bgt },
						{ OpCodes.Bgt_Un_S, OpCodes.Bgt_Un },
						{ OpCodes.Ble_S, OpCodes.Ble },
						{ OpCodes.Ble_Un_S, OpCodes.Ble_Un },
						{ OpCodes.Blt_S, OpCodes.Blt },
						{ OpCodes.Blt_Un_S, OpCodes.Blt_Un },
						{ OpCodes.Bne_Un_S, OpCodes.Bne_Un },
						{ OpCodes.Brfalse_S, OpCodes.Brfalse },
						{ OpCodes.Brtrue_S, OpCodes.Brtrue },
						{ OpCodes.Br_S, OpCodes.Br }
					}.Do(pair =>
					{
						if (instruction.opcode == pair.Key)
							instruction.opcode = pair.Value;
					});

					if (instruction.opcode == OpCodes.Ret)
					{
						instruction.opcode = OpCodes.Br;
						instruction.operand = endLabel;
					}
					return instruction;
				});

			// pass3 - mark labels and emit codes
			codeInstructions.Do(codeInstruction =>
			{
				codeInstruction.labels.ForEach(label => Emitter.MarkLabel(this.generator, label));

                OpCode code = codeInstruction.opcode;
                object operand = codeInstruction.operand;

				if (code.OperandType == OperandType.InlineNone)
					Emitter.Emit(this.generator, code);
				else
				{
					if (operand == null) throw new Exception("Wrong null argument: " + codeInstruction);
                    MethodInfo emitMethod = EmitMethodForType(operand.GetType());
					if (emitMethod == null) throw new Exception("Unknown Emit argument type " + operand.GetType() + " in " + codeInstruction);
					if (HarmonyInstance.debug) FileLog.Log(Emitter.CodePos(this.generator) + code + " " + Emitter.FormatArgument(operand));
					emitMethod.Invoke(this.generator, new object[] { code, operand });
				}
			});
		}

		static void GetMemberInfoValue(MemberInfo info, out object result)
		{
			result = null;
			switch (info.MemberType)
			{
				case MemberTypes.Constructor:
					result = (ConstructorInfo)info;
					break;

				case MemberTypes.Event:
					result = (EventInfo)info;
					break;

				case MemberTypes.Field:
					result = (FieldInfo)info;
					break;

				case MemberTypes.Method:
					result = (MethodInfo)info;
					break;

				case MemberTypes.TypeInfo:
				case MemberTypes.NestedType:
					result = (Type)info;
					break;

				case MemberTypes.Property:
					result = (PropertyInfo)info;
					break;
			}
		}

		// interpret instruction operand

		void ReadOperand(ILInstruction instruction)
		{
			switch (instruction.opcode.OperandType)
			{
				case OperandType.InlineNone:
					{
						instruction.argument = null;
						break;
					}

				case OperandType.InlineSwitch:
					{
                        int length = this.ilBytes.ReadInt32();
                        int base_offset = this.ilBytes.position + (4 * length);
                        int[] branches = new int[length];
						for (int i = 0; i < length; i++)
							branches[i] = this.ilBytes.ReadInt32() + base_offset;
						instruction.operand = branches;
						break;
					}

				case OperandType.ShortInlineBrTarget:
					{
                        sbyte val = (sbyte) this.ilBytes.ReadByte();
						instruction.operand = val + this.ilBytes.position;
						break;
					}

				case OperandType.InlineBrTarget:
					{
                        int val = this.ilBytes.ReadInt32();
						instruction.operand = val + this.ilBytes.position;
						break;
					}

				case OperandType.ShortInlineI:
					{
						if (instruction.opcode == OpCodes.Ldc_I4_S)
						{
                            sbyte sb = (sbyte) this.ilBytes.ReadByte();
							instruction.operand = sb;
							instruction.argument = (sbyte)instruction.operand;
						}
						else
						{
                            byte b = this.ilBytes.ReadByte();
							instruction.operand = b;
							instruction.argument = (byte)instruction.operand;
						}
						break;
					}

				case OperandType.InlineI:
					{
                        int val = this.ilBytes.ReadInt32();
						instruction.operand = val;
						instruction.argument = (int)instruction.operand;
						break;
					}

				case OperandType.ShortInlineR:
					{
                        float val = this.ilBytes.ReadSingle();
						instruction.operand = val;
						instruction.argument = (float)instruction.operand;
						break;
					}

				case OperandType.InlineR:
					{
                        double val = this.ilBytes.ReadDouble();
						instruction.operand = val;
						instruction.argument = (double)instruction.operand;
						break;
					}

				case OperandType.InlineI8:
					{
                        long val = this.ilBytes.ReadInt64();
						instruction.operand = val;
						instruction.argument = (long)instruction.operand;
						break;
					}

				case OperandType.InlineSig:
					{
                        int val = this.ilBytes.ReadInt32();
						instruction.operand = this.module.ResolveSignature(val);
						instruction.argument = (SignatureHelper)instruction.operand;
						break;
					}

				case OperandType.InlineString:
					{
                        int val = this.ilBytes.ReadInt32();
						instruction.operand = this.module.ResolveString(val);
						instruction.argument = (string)instruction.operand;
						break;
					}

				case OperandType.InlineTok:
					{
                        int val = this.ilBytes.ReadInt32();
						instruction.operand = this.module.ResolveMember(val, this.typeArguments, this.methodArguments);
						GetMemberInfoValue((MemberInfo)instruction.operand, out instruction.argument);
						break;
					}

				case OperandType.InlineType:
					{
                        int val = this.ilBytes.ReadInt32();
						instruction.operand = this.module.ResolveType(val, this.typeArguments, this.methodArguments);
						instruction.argument = (Type)instruction.operand;
						break;
					}

				case OperandType.InlineMethod:
					{
                        int val = this.ilBytes.ReadInt32();
						instruction.operand = this.module.ResolveMethod(val, this.typeArguments, this.methodArguments);
						if (instruction.operand is ConstructorInfo)
							instruction.argument = (ConstructorInfo)instruction.operand;
						else
							instruction.argument = (MethodInfo)instruction.operand;
						break;
					}

				case OperandType.InlineField:
					{
                        int val = this.ilBytes.ReadInt32();
						instruction.operand = this.module.ResolveField(val, this.typeArguments, this.methodArguments);
						instruction.argument = (FieldInfo)instruction.operand;
						break;
					}

				case OperandType.ShortInlineVar:
					{
                        byte idx = this.ilBytes.ReadByte();
						if (TargetsLocalVariable(instruction.opcode))
						{
                            LocalVariableInfo lvi = GetLocalVariable(idx);
							if (lvi == null)
								instruction.argument = idx;
							else
							{
								instruction.operand = lvi;
								instruction.argument = this.variables[lvi.LocalIndex];
							}
						}
						else
						{
							instruction.operand = GetParameter(idx);
							instruction.argument = idx;
						}
						break;
					}

				case OperandType.InlineVar:
					{
                        short idx = this.ilBytes.ReadInt16();
						if (TargetsLocalVariable(instruction.opcode))
						{
                            LocalVariableInfo lvi = GetLocalVariable(idx);
							if (lvi == null)
								instruction.argument = idx;
							else
							{
								instruction.operand = lvi;
								instruction.argument = this.variables[lvi.LocalIndex];
							}
						}
						else
						{
							instruction.operand = GetParameter(idx);
							instruction.argument = idx;
						}
						break;
					}

				default:
					throw new NotSupportedException();
			}
		}

		// TODO - implement
		void ParseExceptions()
		{
			foreach (ExceptionHandlingClause ehc in this.exceptions)
			{
				//Log.Error("ExceptionHandlingClause, flags " + ehc.Flags.ToString());

				// The FilterOffset property is meaningful only for Filter
				// clauses. The CatchType property is not meaningful for 
				// Filter or Finally clauses. 
				switch (ehc.Flags)
				{
					case ExceptionHandlingClauseOptions.Filter:
						//Log.Error("    Filter Offset: " + ehc.FilterOffset);
						break;
					case ExceptionHandlingClauseOptions.Finally:
						break;
						//default:
						//Log.Error("Type of exception: " + ehc.CatchType);
						//break;
				}

				//Log.Error("   Handler Length: " + ehc.HandlerLength);
				//Log.Error("   Handler Offset: " + ehc.HandlerOffset);
				//Log.Error(" Try Block Length: " + ehc.TryLength);
				//Log.Error(" Try Block Offset: " + ehc.TryOffset);
			}
		}

		ILInstruction GetInstruction(int offset)
		{
            int lastInstructionIndex = this.ilInstructions.Count - 1;
			if (offset < 0 || offset > this.ilInstructions[lastInstructionIndex].offset)
				throw new Exception("Instruction offset " + offset + " is outside valid range 0 - " + this.ilInstructions[lastInstructionIndex].offset);

			int min = 0;
			int max = lastInstructionIndex;
			while (min <= max)
			{
				int mid = min + ((max - min) / 2);
                ILInstruction instruction = this.ilInstructions[mid];
                int instruction_offset = instruction.offset;

				if (offset == instruction_offset)
					return instruction;

				if (offset < instruction_offset)
					max = mid - 1;
				else
					min = mid + 1;
			}

			throw new Exception("Cannot find instruction for " + offset);
		}

        static bool TargetsLocalVariable(OpCode opcode) => opcode.Name.Contains("loc");

        LocalVariableInfo GetLocalVariable(int index) => this.locals?[index];

        ParameterInfo GetParameter(int index)
		{
			if (index == 0)
				return this.this_parameter;

			return this.parameters[index - 1];
		}

		OpCode ReadOpCode()
		{
            byte op = this.ilBytes.ReadByte();
			return op != 0xfe
				? one_byte_opcodes[op]
				: two_bytes_opcodes[this.ilBytes.ReadByte()];
		}

		MethodInfo EmitMethodForType(Type type)
		{
			foreach (KeyValuePair<Type, MethodInfo> entry in emitMethods)
				if (entry.Key == type) return entry.Value;
			foreach (KeyValuePair<Type, MethodInfo> entry in emitMethods)
				if (entry.Key.IsAssignableFrom(type)) return entry.Value;
			return null;
		}

		// static initializer to prep opcodes

		static readonly OpCode[] one_byte_opcodes;
		static readonly OpCode[] two_bytes_opcodes;

		static readonly Dictionary<Type, MethodInfo> emitMethods;

		[MethodImpl(MethodImplOptions.Synchronized)]
		static MethodBodyReader()
		{
			one_byte_opcodes = new OpCode[0xe1];
			two_bytes_opcodes = new OpCode[0x1f];

            FieldInfo[] fields = typeof(OpCodes).GetFields(
				BindingFlags.Public | BindingFlags.Static);

			foreach (FieldInfo field in fields)
			{
                OpCode opcode = (OpCode)field.GetValue(null);
				if (opcode.OpCodeType == OpCodeType.Nternal)
					continue;

				if (opcode.Size == 1)
					one_byte_opcodes[opcode.Value] = opcode;
				else
					two_bytes_opcodes[opcode.Value & 0xff] = opcode;
			}

			emitMethods = new Dictionary<Type, MethodInfo>();
			typeof(ILGenerator).GetMethods().ToList()
				.ForEach(method =>
				{
					if (method.Name != "Emit") return;
                    ParameterInfo[] pinfos = method.GetParameters();
					if (pinfos.Length != 2) return;
                    Type[] types = pinfos.Select(p => p.ParameterType).ToArray();
					if (types[0] != typeof(OpCode)) return;
					emitMethods[types[1]] = method;
				});
		}

		// a custom this parameter

		class ThisParameter : ParameterInfo
		{
			public ThisParameter(MethodBase method)
			{
                this.MemberImpl = method;
                this.ClassImpl = method.DeclaringType;
                this.NameImpl = "this";
                this.PositionImpl = -1;
			}
		}
	}
}