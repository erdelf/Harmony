using Harmony;
using Harmony.ILCopying;
using HarmonyTests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace HarmonyTests
{
	[TestClass]
	public class StaticPatches
	{
		[TestMethod]
		public void TestMethod1()
		{
            Type originalClass = typeof(Class1);
			Assert.IsNotNull(originalClass);
            MethodInfo originalMethod = originalClass.GetMethod("Method1");
			Assert.IsNotNull(originalMethod);

            Type patchClass = typeof(Class1Patch);
            MethodInfo realPrefix = patchClass.GetMethod("Prefix");
            MethodInfo realPostfix = patchClass.GetMethod("Postfix");
            MethodInfo realTranspiler = patchClass.GetMethod("Transpiler");
			Assert.IsNotNull(realPrefix);
			Assert.IsNotNull(realPostfix);
			Assert.IsNotNull(realTranspiler);

			Class1Patch._reset();
            PatchTools.GetPatches(typeof(Class1Patch), originalMethod, out MethodInfo prefixMethod, out MethodInfo postfixMethod, out MethodInfo transpilerMethod);

            Assert.AreSame(realPrefix, prefixMethod);
			Assert.AreSame(realPostfix, postfixMethod);
			Assert.AreSame(realTranspiler, transpilerMethod);

            HarmonyInstance instance = HarmonyInstance.Create("test");
			Assert.IsNotNull(instance);

            PatchProcessor patcher = new PatchProcessor(instance, originalMethod, new HarmonyMethod(prefixMethod), new HarmonyMethod(postfixMethod), new HarmonyMethod(transpilerMethod));
			Assert.IsNotNull(patcher);

            long originalMethodStartPre = Memory.GetMethodStart(originalMethod);
			patcher.Patch();
            long originalMethodStartPost = Memory.GetMethodStart(originalMethod);
			Assert.AreEqual(originalMethodStartPre, originalMethodStartPost);
			unsafe
			{
				byte patchedCode = *(byte*) originalMethodStartPre;
				if (IntPtr.Size == sizeof(long))
					Assert.IsTrue(patchedCode == 0x48);
				else
					Assert.IsTrue(patchedCode == 0x68);
			}

			Class1.Method1();
			Assert.IsTrue(Class1Patch.prefixed);
			Assert.IsTrue(Class1Patch.originalExecuted);
			Assert.IsTrue(Class1Patch.postfixed);
		}

		[TestMethod]
		public void TestMethod2()
		{
            Type originalClass = typeof(Class2);
			Assert.IsNotNull(originalClass);
            MethodInfo originalMethod = originalClass.GetMethod("Method2");
			Assert.IsNotNull(originalMethod);

            Type patchClass = typeof(Class2Patch);
            MethodInfo realPrefix = patchClass.GetMethod("Prefix");
            MethodInfo realPostfix = patchClass.GetMethod("Postfix");
            MethodInfo realTranspiler = patchClass.GetMethod("Transpiler");
			Assert.IsNotNull(realPrefix);
			Assert.IsNotNull(realPostfix);
			Assert.IsNotNull(realTranspiler);

			Class2Patch._reset();
            PatchTools.GetPatches(typeof(Class2Patch), originalMethod, out MethodInfo prefixMethod, out MethodInfo postfixMethod, out MethodInfo transpilerMethod);

            Assert.AreSame(realPrefix, prefixMethod);
			Assert.AreSame(realPostfix, postfixMethod);
			Assert.AreSame(realTranspiler, transpilerMethod);

            HarmonyInstance instance = HarmonyInstance.Create("test");
			Assert.IsNotNull(instance);

            PatchProcessor patcher = new PatchProcessor(instance, originalMethod, new HarmonyMethod(prefixMethod), new HarmonyMethod(postfixMethod), new HarmonyMethod(transpilerMethod));
			Assert.IsNotNull(patcher);

            long originalMethodStartPre = Memory.GetMethodStart(originalMethod);
			patcher.Patch();
            long originalMethodStartPost = Memory.GetMethodStart(originalMethod);
			Assert.AreEqual(originalMethodStartPre, originalMethodStartPost);
			unsafe
			{
				byte patchedCode = *(byte*) originalMethodStartPre;
				if (IntPtr.Size == sizeof(long))
					Assert.IsTrue(patchedCode == 0x48);
				else
					Assert.IsTrue(patchedCode == 0x68);
			}

			new Class2().Method2();
			Assert.IsTrue(Class2Patch.prefixed);
			Assert.IsTrue(Class2Patch.originalExecuted);
			Assert.IsTrue(Class2Patch.postfixed);
		}
	}
}