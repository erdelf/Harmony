using HarmonyErdelf.ILCopying;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace HarmonyErdelf
{
	public static class DynamicTools
	{
		public static DynamicMethod CreateDynamicMethod(MethodBase original, string suffix)
		{
			if (original == null) throw new Exception("original cannot be null");
            string patchName = original.Name + suffix;
			patchName = patchName.Replace("<>", "");

            ParameterInfo[] parameters = original.GetParameters();
            System.Collections.Generic.List<Type> result = parameters.Types().ToList();
			if (original.IsStatic == false)
				result.Insert(0, typeof(object));
            Type[] paramTypes = result.ToArray();

            DynamicMethod method = new DynamicMethod(
				patchName,
				MethodAttributes.Public | MethodAttributes.Static,
				CallingConventions.Standard,
				AccessTools.GetReturnedType(original),
				paramTypes,
				original.DeclaringType,
				true
			);

			for (int i = 0; i < parameters.Length; i++)
				method.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);

			return method;
		}

        public static LocalBuilder[] DeclareLocalVariables(MethodBase original, ILGenerator il) => 
            original.GetMethodBody().LocalVariables.Select(lvi =>
            {
                LocalBuilder localBuilder = il.DeclareLocal(lvi.LocalType, lvi.IsPinned);
                Emitter.LogLastLocalVariable(il);
                return localBuilder;
            }).ToArray();

        public static LocalBuilder DeclareLocalVariable(ILGenerator il, Type type)
		{
			if (type.IsByRef) type = type.GetElementType();

			if (AccessTools.IsClass(type))
			{
                LocalBuilder v = il.DeclareLocal(type);
				Emitter.LogLastLocalVariable(il);
				Emitter.Emit(il, OpCodes.Ldnull);
				Emitter.Emit(il, OpCodes.Stloc, v);
				return v;
			}
			if (AccessTools.IsStruct(type))
			{
                LocalBuilder v = il.DeclareLocal(type);
				Emitter.LogLastLocalVariable(il);
				Emitter.Emit(il, OpCodes.Ldloca, v);
				Emitter.Emit(il, OpCodes.Initobj, type);
				return v;
			}
			if (AccessTools.IsValue(type))
			{
                LocalBuilder v = il.DeclareLocal(type);
				Emitter.LogLastLocalVariable(il);
				if (type == typeof(float))
					Emitter.Emit(il, OpCodes.Ldc_R4, (float)0);
				else if (type == typeof(double))
					Emitter.Emit(il, OpCodes.Ldc_R8, (double)0);
				else if (type == typeof(long))
					Emitter.Emit(il, OpCodes.Ldc_I8, (long)0);
				else
					Emitter.Emit(il, OpCodes.Ldc_I4, 0);
				Emitter.Emit(il, OpCodes.Stloc, v);
				return v;
			}
			return null;
		}

		public static void PrepareDynamicMethod(DynamicMethod method)
		{
            BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
            BindingFlags nonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;

            // on mono, just call 'CreateDynMethod'
            //
            MethodInfo m_CreateDynMethod = typeof(DynamicMethod).GetMethod("CreateDynMethod", nonPublicInstance);
			if (m_CreateDynMethod != null)
			{
				m_CreateDynMethod.Invoke(method, new object[0]);
				return;
			}

            // on all .NET Core versions, call 'RuntimeHelpers._CompileMethod' but with a different parameter:
            //
            MethodInfo m__CompileMethod = typeof(RuntimeHelpers).GetMethod("_CompileMethod", nonPublicStatic);

            MethodInfo m_GetMethodDescriptor = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", nonPublicInstance);
            RuntimeMethodHandle handle = (RuntimeMethodHandle)m_GetMethodDescriptor.Invoke(method, new object[0]);

            // 1) RuntimeHelpers._CompileMethod(handle.GetMethodInfo())
            //
            MethodInfo m_GetMethodInfo = typeof(RuntimeMethodHandle).GetMethod("GetMethodInfo", nonPublicInstance);
			if (m_GetMethodInfo != null)
			{
                object runtimeMethodInfo = m_GetMethodInfo.Invoke(handle, new object[0]);
				m__CompileMethod.Invoke(null, new object[] { runtimeMethodInfo });
				return;
			}

			// 2) RuntimeHelpers._CompileMethod(handle.Value)
			//
			if (m__CompileMethod.GetParameters()[0].ParameterType == typeof(IntPtr))
			{
				m__CompileMethod.Invoke(null, new object[] { handle.Value });
				return;
			}

			// 3) RuntimeHelpers._CompileMethod(handle)
			//
			m__CompileMethod.Invoke(null, new object[] { handle });
		}
	}
}