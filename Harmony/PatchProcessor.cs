using System;
using System.Collections.Generic;
using System.Reflection;

namespace Harmony
{
	public class PatchProcessor
	{
		static object locker = new object();

		readonly HarmonyInstance instance;

		MethodBase original;
		HarmonyMethod prefix;
		HarmonyMethod postfix;
		HarmonyMethod transpiler;

		public PatchProcessor(HarmonyInstance instance, MethodBase original, HarmonyMethod prefix, HarmonyMethod postfix, HarmonyMethod transpiler)
		{
			this.instance = instance;
			this.original = original;
			this.prefix = prefix ?? new HarmonyMethod(null);
			this.postfix = postfix ?? new HarmonyMethod(null);
			this.transpiler = transpiler ?? new HarmonyMethod(null);
		}

        public static PatchesInfo IsPatched(MethodBase method) => HarmonySharedState.GetPatchInfo(method);

        public static IEnumerable<MethodBase> AllPatchedMethods() => HarmonySharedState.GetPatchedMethods();

        public void Patch()
		{
			lock (locker)
			{
                PatchesInfo patchInfo = HarmonySharedState.GetPatchInfo(this.original);
				if (patchInfo == null) patchInfo = new PatchesInfo();

				PatchFunctions.AddPrefix(patchInfo, this.instance.Id, this.prefix);
				PatchFunctions.AddPostfix(patchInfo, this.instance.Id, this.postfix);
				PatchFunctions.AddTranspiler(patchInfo, this.instance.Id, this.transpiler);
				PatchFunctions.UpdateWrapper(this.original, patchInfo);

				HarmonySharedState.UpdatePatchInfo(this.original, patchInfo);
			}
		}
	}
}