using System;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyErdelf
{
	class DelegateTypeFactory
	{
		readonly ModuleBuilder module;

		static int counter;
		public DelegateTypeFactory()
		{
			counter++;
            AssemblyName name = new AssemblyName("HarmonyDTFAssembly" + counter);
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            this.module = assembly.DefineDynamicModule("HarmonyDTFModule" + counter);
		}

		public Type CreateDelegateType(MethodInfo method)
		{
            TypeAttributes attr = TypeAttributes.Sealed | TypeAttributes.Public;
            TypeBuilder typeBuilder = this.module.DefineType("HarmonyDTFType" + counter, attr, typeof(MulticastDelegate));

            ConstructorBuilder constructor = typeBuilder.DefineConstructor(
				 MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
				 CallingConventions.Standard, new[] { typeof(object), typeof(IntPtr) });
			constructor.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            ParameterInfo[] parameters = method.GetParameters();

            MethodBuilder invokeMethod = typeBuilder.DefineMethod(
				 "Invoke", MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public,
				 method.ReturnType, parameters.Types());
			invokeMethod.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

			for (int i = 0; i < parameters.Length; i++)
				invokeMethod.DefineParameter(i + 1, ParameterAttributes.None, parameters[i].Name);

			return typeBuilder.CreateType();
		}
	}
}