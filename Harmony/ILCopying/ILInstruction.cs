using System.Collections.Generic;
using System.Reflection.Emit;

namespace Harmony.ILCopying
{
	public class ILInstruction
	{
		public int offset;
		public OpCode opcode;
		public object operand;
		public object argument;

		public List<Label> labels = new List<Label>();

		public ILInstruction(OpCode opcode, object operand = null)
		{
			this.opcode = opcode;
			this.operand = operand;
			this.argument = operand;
		}

		public CodeInstruction GetCodeInstruction()
		{
            CodeInstruction instr = new CodeInstruction(this.opcode, this.argument);
			if (this.opcode.OperandType == OperandType.InlineNone)
				instr.operand = null;
			instr.labels = this.labels;
			return instr;
		}

		public int GetSize()
		{
			int size = this.opcode.Size;

			switch (this.opcode.OperandType)
			{
				case OperandType.InlineSwitch:
					size += (1 + ((int[]) this.operand).Length) * 4;
					break;

				case OperandType.InlineI8:
				case OperandType.InlineR:
					size += 8;
					break;

				case OperandType.InlineBrTarget:
				case OperandType.InlineField:
				case OperandType.InlineI:
				case OperandType.InlineMethod:
				case OperandType.InlineString:
				case OperandType.InlineTok:
				case OperandType.InlineType:
				case OperandType.ShortInlineR:
					size += 4;
					break;

				case OperandType.InlineVar:
					size += 2;
					break;

				case OperandType.ShortInlineBrTarget:
				case OperandType.ShortInlineI:
				case OperandType.ShortInlineVar:
					size += 1;
					break;
			}

			return size;
		}

		public override string ToString()
		{
            string instruction = "";

			AppendLabel(ref instruction, this);
			instruction = instruction + ": " + this.opcode.Name;

			if (this.operand == null)
				return instruction;

			instruction = instruction + " ";

			switch (this.opcode.OperandType)
			{
				case OperandType.ShortInlineBrTarget:
				case OperandType.InlineBrTarget:
					AppendLabel(ref instruction, this.operand);
					break;

				case OperandType.InlineSwitch:
                    ILInstruction[] switchLabels = (ILInstruction[]) this.operand;
					for (int i = 0; i < switchLabels.Length; i++)
					{
						if (i > 0)
							instruction = instruction + ",";

						AppendLabel(ref instruction, switchLabels[i]);
					}
					break;

				case OperandType.InlineString:
					instruction = instruction + "\"" + this.operand + "\"";
					break;

				default:
					instruction = instruction + this.operand;
					break;
			}

			return instruction;
		}

		static void AppendLabel(ref string str, object argument)
		{
            if (argument is ILInstruction instruction)
                str = str + "IL_" + instruction.offset.ToString("X4");
            else
                str = str + "IL_" + argument;
        }
	}
}
