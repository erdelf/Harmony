﻿using Harmony.ILCopying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Harmony
{
	public static class PatchFunctions
	{
		public static void AddPrefix(PatchInfo patchInfo, string owner, HarmonyMethod info)
		{
			if (info == null || info.method == null) return;

            int priority = info.prioritiy == -1 ? Priority.Normal : info.prioritiy;
            string[] before = info.before ?? new string[0];
            string[] after = info.after ?? new string[0];

			patchInfo.AddPrefix(info.method, owner, priority, before, after);
		}

		public static void AddPostfix(PatchInfo patchInfo, string owner, HarmonyMethod info)
		{
			if (info == null || info.method == null) return;

            int priority = info.prioritiy == -1 ? Priority.Normal : info.prioritiy;
            string[] before = info.before ?? new string[0];
            string[] after = info.after ?? new string[0];

			patchInfo.AddPostfix(info.method, owner, priority, before, after);
		}

		public static void AddTranspiler(PatchInfo patchInfo, string owner, HarmonyMethod info)
		{
			if (info == null || info.method == null) return;

            int priority = info.prioritiy == -1 ? Priority.Normal : info.prioritiy;
            string[] before = info.before ?? new string[0];
            string[] after = info.after ?? new string[0];

			patchInfo.AddTranspiler(info.method, owner, priority, before, after);
		}

        public static List<MethodInfo> GetSortedPatchMethods(MethodBase original, Patch[] patches) => patches
                .Where(p => p.patch != null)
                .OrderBy(p => p)
                .Select(p => p.GetMethod(original))
                .ToList();

        public static void UpdateWrapper(MethodBase original, PatchInfo patchInfo)
		{
            List<MethodInfo> sortedPrefixes = GetSortedPatchMethods(original, patchInfo.prefixes);
            List<MethodInfo> sortedPostfixes = GetSortedPatchMethods(original, patchInfo.postfixes);
            List<MethodInfo> sortedTranspilers = GetSortedPatchMethods(original, patchInfo.transpilers);

            System.Reflection.Emit.DynamicMethod replacement = MethodPatcher.CreatePatchedMethod(original, sortedPrefixes, sortedPostfixes, sortedTranspilers);
			if (replacement == null) throw new MissingMethodException("Cannot create dynamic replacement for " + original);

            long originalCodeStart = Memory.GetMethodStart(original);
            long patchCodeStart = Memory.GetMethodStart(replacement);
			Memory.WriteJump(originalCodeStart, patchCodeStart);

			PatchTools.RememberObject(original, replacement); // no gc for new value + release old value to gc
		}
	}
}