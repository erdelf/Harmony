using System;
using System.Collections.Generic;
using System.Reflection;

namespace HarmonyErdelf
{
    public class HarmonyInstance
	{
		readonly string id;
		public string Id => this.id;
		public static bool debug = false;

        HarmonyInstance(string id) => this.id = id;

        public static HarmonyInstance Create(string id)
		{
			if (id == null) throw new Exception("id cannot be null");
			return new HarmonyInstance(id);
		}

        public void Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null) => 
            new PatchProcessor(this, original, prefix, postfix, transpiler).Patch();

        public PatchesInfo GetPatches(MethodBase method) => HarmonySharedState.GetPatchInfo(method);

        public IEnumerable<MethodBase> GetPatchedMethods() => HarmonySharedState.GetPatchedMethods();
	}
}