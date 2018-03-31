using Harmony;
using Harmony.ILCopying;
using HarmonyTests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace HarmonyTests
{
	[TestClass]
	public class TestMethodInvoker
    {

		[TestMethod]
        public void TestMethodInvokerGeneral()
        {
            Type type = typeof(MethodInvokerClass);
            Assert.IsNotNull(type);
            MethodInfo method = type.GetMethod("Method1");
            Assert.IsNotNull(method);

            FastInvokeHandler handler = MethodInvoker.GetHandler(method);
            Assert.IsNotNull(handler);

            object[] args = new object[] { 1, 0, 0, /*out*/ null, /*ref*/ new TestMethodInvokerStruct() };
            handler(null, args);
            Assert.AreEqual(args[0], 1);
            Assert.AreEqual(args[1], 1);
            Assert.AreEqual(args[2], 2);
            Assert.AreEqual(((TestMethodInvokerObject) args[3])?.value, 1);
            Assert.AreEqual(((TestMethodInvokerStruct) args[4]).value, 1);
        }

        [TestMethod]
        public void TestMethodInvokerSelfObject()
        {
            Type type = typeof(TestMethodInvokerObject);
            Assert.IsNotNull(type);
            MethodInfo method = type.GetMethod("Method1");
            Assert.IsNotNull(method);

            FastInvokeHandler handler = MethodInvoker.GetHandler(method);
            Assert.IsNotNull(handler);

            TestMethodInvokerObject instance = new TestMethodInvokerObject
            {
                value = 1
            };

            object[] args = new object[] { 2 };
            handler(instance, args);
            Assert.AreEqual(instance.value, 3);
        }

    }
}