using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Harmony
{
	public class Patches
	{
		public readonly ReadOnlyCollection<Patch> prefixes;
		public readonly ReadOnlyCollection<Patch> postfixes;
		public readonly ReadOnlyCollection<Patch> transpilers;

		public ReadOnlyCollection<string> Owners
		{
			get
			{
                HashSet<string> result = new HashSet<string>();
				result.UnionWith(this.prefixes.Select(p => p.owner));
				result.UnionWith(this.postfixes.Select(p => p.owner));
				result.UnionWith(this.postfixes.Select(p => p.owner));
				return result.ToList().AsReadOnly();
			}
		}

		public Patches(Patch[] prefixes, Patch[] postfixes, Patch[] transpilers)
		{
			if (prefixes == null) prefixes = new Patch[0];
			if (postfixes == null) postfixes = new Patch[0];
			if (transpilers == null) transpilers = new Patch[0];

            this.prefixes = prefixes.ToList().AsReadOnly();
            this.postfixes = postfixes.ToList().AsReadOnly();
            this.transpilers = transpilers.ToList().AsReadOnly();
		}
	}

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

        //

        public void PatchAll(Assembly assembly) => assembly.GetTypes().Do(type =>
                                                   {
                                                       List<HarmonyMethod> parentMethodInfos = type.GetHarmonyMethods();
                                                       if (parentMethodInfos != null && parentMethodInfos.Count() > 0)
                                                       {
                                                           HarmonyMethod info = HarmonyMethod.Merge(parentMethodInfos);
                                                           PatchProcessor processor = new PatchProcessor(this, type, info);
                                                           processor.Patch();
                                                       }
                                                   });

        public void Patch(MethodBase original, HarmonyMethod prefix, HarmonyMethod postfix, HarmonyMethod transpiler = null)
		{
            PatchProcessor processor = new PatchProcessor(this, original, prefix, postfix, transpiler);
			processor.Patch();
		}

        //

        public Patches IsPatched(MethodBase method) => PatchProcessor.IsPatched(method);

        public IEnumerable<MethodBase> GetPatchedMethods() => HarmonySharedState.GetPatchedMethods();

        public Dictionary<string, Version> VersionInfo(out Version currentVersion)
		{
			currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
			GetPatchedMethods().Do(method =>
			{
                PatchInfo info = HarmonySharedState.GetPatchInfo(method);
				info.prefixes.Do(fix => assemblies[fix.owner] = fix.patch.DeclaringType.Assembly);
				info.postfixes.Do(fix => assemblies[fix.owner] = fix.patch.DeclaringType.Assembly);
				info.transpilers.Do(fix => assemblies[fix.owner] = fix.patch.DeclaringType.Assembly);
			});

            Dictionary<string, Version> result = new Dictionary<string, Version>();
			assemblies.Do(info =>
			{
                AssemblyName assemblyName = info.Value.GetReferencedAssemblies().FirstOrDefault(a => a.FullName.StartsWith("0Harmony, Version"));
				if (assemblyName != null)
					result[info.Key] = assemblyName.Version;
			});
			return result;
		}
	}
}