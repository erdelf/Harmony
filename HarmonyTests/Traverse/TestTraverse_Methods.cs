using Harmony;
using HarmonyTests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HarmonyTests
{
	[TestClass]
	public class TestTraverse_Methods
	{
		[TestMethod]
		public void Traverse_Missing_Method()
		{
            TraverseMethods_Instance instance = new TraverseMethods_Instance();
            Traverse trv = Traverse.Create(instance);

			string methodSig1 = "";
			try
			{
				trv.Method("FooBar", new object[] { "hello", 123 });
			}
			catch (MissingMethodException e)
			{
				methodSig1 = e.Message;
			}
			Assert.AreEqual("FooBar(System.String, System.Int32)", methodSig1);

			string methodSig2 = "";
			try
			{
                Type[] types = new Type[] { typeof(string), typeof(int) };
				trv.Method("FooBar", types, new object[] { "hello", 123 });
			}
			catch (MissingMethodException e)
			{
				methodSig2 = e.Message;
			}
			Assert.AreEqual("FooBar(System.String, System.Int32)", methodSig2);
		}

		[TestMethod]
		public void Traverse_Method_Instance()
		{
            TraverseMethods_Instance instance = new TraverseMethods_Instance();
            Traverse trv = Traverse.Create(instance);

			instance.method1_called = false;
            Traverse mtrv1 = trv.Method("Method1");
			Assert.AreEqual(null, mtrv1.GetValue());
			Assert.AreEqual(true, instance.method1_called);

            Traverse mtrv2 = trv.Method("Method2", new object[] { "arg" });
			Assert.AreEqual("argarg", mtrv2.GetValue());
		}

		[TestMethod]
		public void Traverse_Method_Static()
		{
            Traverse trv = Traverse.Create(typeof(TraverseMethods_Static));
            Traverse mtrv = trv.Method("StaticMethod", new object[] { 6, 7 });
			Assert.AreEqual(42, mtrv.GetValue<int>());
		}

		[TestMethod]
		public void Traverse_Method_VariableArguments()
		{
            Traverse trv = Traverse.Create(typeof(TraverseMethods_VarArgs));

			Assert.AreEqual(30, trv.Method("Test1", 10, 20).GetValue<int>());
			Assert.AreEqual(60, trv.Method("Test2", 10, 20, 30).GetValue<int>());

			// Calling varargs methods directly won't work. Use parameter array instead
			// Assert.AreEqual(60, trv.Method("Test3", 100, 10, 20, 30).GetValue<int>());
			Assert.AreEqual(6000, trv.Method("Test3", 100, new int[] { 10, 20, 30 }).GetValue<int>());
		}

		[TestMethod]
		public void Traverse_Method_RefParameters()
		{
            Traverse trv = Traverse.Create(typeof(TraverseMethods_Parameter));

			string result = null;
            object[] parameters = new object[] { result };
            Type[] types = new Type[] { typeof(string).MakeByRefType() };
            Traverse mtrv1 = trv.Method("WithRefParameter", types, parameters);
			Assert.AreEqual("ok", mtrv1.GetValue<string>());
			Assert.AreEqual("hello", parameters[0]);
		}

		[TestMethod]
		public void Traverse_Method_OutParameters()
		{
            Traverse trv = Traverse.Create(typeof(TraverseMethods_Parameter));

			string result = null;
            object[] parameters = new object[] { result };
            Type[] types = new Type[] { typeof(string).MakeByRefType() };
            Traverse mtrv1 = trv.Method("WithOutParameter", types, parameters);
			Assert.AreEqual("ok", mtrv1.GetValue<string>());
			Assert.AreEqual("hello", parameters[0]);
		}
	}
}