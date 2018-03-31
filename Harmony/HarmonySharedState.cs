using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Harmony
{
	public static class HarmonySharedState
	{
		static readonly string name = "HarmonySharedState";
		internal static readonly int internalVersion = 100;
		internal static int actualVersion = -1;

		static Dictionary<MethodBase, byte[]> GetState()
		{
			lock (name)
			{
                Assembly assembly = SharedStateAssembly();
				if (assembly == null)
				{
                    AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);
                    ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(name);
                    TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Abstract;
                    TypeBuilder typeBuilder = moduleBuilder.DefineType(name, typeAttributes);
					typeBuilder.DefineField("state", typeof(Dictionary<MethodBase, byte[]>), FieldAttributes.Static | FieldAttributes.Public);
					typeBuilder.DefineField("version", typeof(int), FieldAttributes.Static | FieldAttributes.Public).SetConstant(internalVersion);
					typeBuilder.CreateType();

					assembly = SharedStateAssembly();
					if (assembly == null) throw new Exception("Cannot find or create harmony shared state");
				}

                FieldInfo versionField = assembly.GetType(name).GetField("version");
				if (versionField == null) throw new Exception("Cannot find harmony state version field");
				actualVersion = (int)versionField.GetValue(null);

                FieldInfo stateField = assembly.GetType(name).GetField("state");
				if (stateField == null) throw new Exception("Cannot find harmony state field");
				if (stateField.GetValue(null) == null) stateField.SetValue(null, new Dictionary<MethodBase, byte[]>());
				return (Dictionary<MethodBase, byte[]>)stateField.GetValue(null);
			}
		}

        static Assembly SharedStateAssembly() => AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name.Contains(name));

        internal static PatchesInfo GetPatchInfo(MethodBase method)
		{
            byte[] bytes = GetState().GetValueSafe(method);
			if (bytes == null) return null;
			return PatchInfoSerialization.Deserialize(bytes);
		}

        internal static IEnumerable<MethodBase> GetPatchedMethods() => GetState().Keys.AsEnumerable();

        internal static void UpdatePatchInfo(MethodBase method, PatchesInfo patchInfo) => GetState()[method] = patchInfo.Serialize();
    }
}