using System;
using System.Collections.Generic;
using System.Reflection;

namespace Harmony
{
	public class PatchProcessor
	{
		static object locker = new object();

		readonly HarmonyInstance instance;

		readonly Type container;
		readonly HarmonyMethod containerAttributes;

		MethodBase original;
		HarmonyMethod prefix;
		HarmonyMethod postfix;
		HarmonyMethod transpiler;

		public PatchProcessor(HarmonyInstance instance, Type type, HarmonyMethod attributes)
		{
			this.instance = instance;
            this.container = type;
            this.containerAttributes = attributes ?? new HarmonyMethod(null);
            this.prefix = this.containerAttributes.Clone();
            this.postfix = this.containerAttributes.Clone();
            this.transpiler = this.containerAttributes.Clone();
			ProcessType();
		}

		public PatchProcessor(HarmonyInstance instance, MethodBase original, HarmonyMethod prefix, HarmonyMethod postfix, HarmonyMethod transpiler)
		{
			this.instance = instance;
			this.original = original;
			this.prefix = prefix ?? new HarmonyMethod(null);
			this.postfix = postfix ?? new HarmonyMethod(null);
			this.transpiler = transpiler ?? new HarmonyMethod(null);
		}

		public static Patches IsPatched(MethodBase method)
		{
            PatchInfo patchInfo = HarmonySharedState.GetPatchInfo(method);
			if (patchInfo == null) return null;
			return new Patches(patchInfo.prefixes, patchInfo.postfixes, patchInfo.transpilers);
		}

        public static IEnumerable<MethodBase> AllPatchedMethods() => HarmonySharedState.GetPatchedMethods();

        public void Patch()
		{
			lock (locker)
			{
                PatchInfo patchInfo = HarmonySharedState.GetPatchInfo(this.original);
				if (patchInfo == null) patchInfo = new PatchInfo();

				PatchFunctions.AddPrefix(patchInfo, this.instance.Id, this.prefix);
				PatchFunctions.AddPostfix(patchInfo, this.instance.Id, this.postfix);
				PatchFunctions.AddTranspiler(patchInfo, this.instance.Id, this.transpiler);
				PatchFunctions.UpdateWrapper(this.original, patchInfo);

				HarmonySharedState.UpdatePatchInfo(this.original, patchInfo);
			}
		}

		bool CallPrepare()
		{
			if (this.original != null)
				return RunMethod<HarmonyPrepare, bool>(true, this.original);
			return RunMethod<HarmonyPrepare, bool>(true);
		}

		void ProcessType()
		{
            this.original = GetOriginalMethod();

            bool patchable = CallPrepare();
			if (patchable)
			{
				if (this.original == null)
                    this.original = RunMethod<HarmonyTargetMethod, MethodBase>(null);
				if (this.original == null)
					throw new ArgumentException("No target method specified for class " + this.container.FullName);

				PatchTools.GetPatches(this.container, this.original, out this.prefix.method, out this.postfix.method, out this.transpiler.method);

				if (this.prefix.method != null)
				{
					if (this.prefix.method.IsStatic == false)
						throw new ArgumentException("Patch method " + this.prefix.method.Name + " in " + this.prefix.method.DeclaringType + " must be static");

                    List<HarmonyMethod> prefixAttributes = this.prefix.method.GetHarmonyMethods();
                    this.containerAttributes.Merge(HarmonyMethod.Merge(prefixAttributes)).CopyTo(this.prefix);
				}

				if (this.postfix.method != null)
				{
					if (this.postfix.method.IsStatic == false)
						throw new ArgumentException("Patch method " + this.postfix.method.Name + " in " + this.postfix.method.DeclaringType + " must be static");

                    List<HarmonyMethod> postfixAttributes = this.postfix.method.GetHarmonyMethods();
                    this.containerAttributes.Merge(HarmonyMethod.Merge(postfixAttributes)).CopyTo(this.postfix);
				}

				if (this.transpiler.method != null)
				{
					if (this.transpiler.method.IsStatic == false)
						throw new ArgumentException("Patch method " + this.transpiler.method.Name + " in " + this.transpiler.method.DeclaringType + " must be static");

                    List<HarmonyMethod> infixAttributes = this.transpiler.method.GetHarmonyMethods();
                    this.containerAttributes.Merge(HarmonyMethod.Merge(infixAttributes)).CopyTo(this.transpiler);
				}
			}
		}

		MethodBase GetOriginalMethod()
		{
            HarmonyMethod attr = this.containerAttributes;
			if (attr.originalType == null) return null;
			if (attr.methodName == null)
				return AccessTools.Constructor(attr.originalType, attr.parameter);
			return AccessTools.Method(attr.originalType, attr.methodName, attr.parameter);
		}

		T RunMethod<S, T>(T defaultIfNotExisting, params object[] parameters)
		{
            string methodName = typeof(S).Name.Replace("Harmony", "");

            List<object> paramList = new List<object> { this.instance };
			paramList.AddRange(parameters);
            Type[] paramTypes = AccessTools.GetTypes(paramList.ToArray());
            MethodInfo method = PatchTools.GetPatchMethod<S>(this.container, methodName, paramTypes);
			if (method != null && typeof(T).IsAssignableFrom(method.ReturnType))
				return (T)method.Invoke(null, paramList.ToArray());

			method = PatchTools.GetPatchMethod<S>(this.container, methodName, new Type[] { typeof(HarmonyInstance) });
			if (method != null && typeof(T).IsAssignableFrom(method.ReturnType))
				return (T)method.Invoke(null, new object[] { this.instance });

			method = PatchTools.GetPatchMethod<S>(this.container, methodName, Type.EmptyTypes);
			if (method != null)
			{
				if (typeof(T).IsAssignableFrom(method.ReturnType))
					return (T)method.Invoke(null, Type.EmptyTypes);

				method.Invoke(null, Type.EmptyTypes);
				return defaultIfNotExisting;
			}

			return defaultIfNotExisting;
		}
	}
}