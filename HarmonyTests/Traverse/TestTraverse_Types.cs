using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HarmonyTests.Assets;
using Harmony;

namespace HarmonyTests
{
	[TestClass]
	public class TestTraverse_Types
	{
		private class InnerClass { }

		[TestMethod]
		public void Traverse_Types()
		{
            TraverseTypes<InnerClass> instance = new Assets.TraverseTypes<InnerClass>();
            Traverse trv = Traverse.Create(instance);

			Assert.AreEqual(
				100,
				trv.Field("IntField").GetValue<int>()
			);

			Assert.AreEqual(
				"hello",
				trv.Field("StringField").GetValue<string>()
			);

            bool[] boolArray = trv.Field("ListOfBoolField").GetValue<IEnumerable<bool>>().ToArray();
			Assert.AreEqual(true, boolArray[0]);
			Assert.AreEqual(false, boolArray[1]);

            Dictionary<InnerClass, List<string>> mixed = trv.Field("MixedField").GetValue<Dictionary<InnerClass, List<string>>>();
            InnerClass key = trv.Field("key").GetValue<InnerClass>();

            mixed.TryGetValue(key, out List<string> value);
            Assert.AreEqual("world", value.First());

            Traverse trvEmpty = Traverse.Create(instance).Type("FooBar");
			TestTraverse_Basics.AssertIsEmpty(trvEmpty);
		}

		[TestMethod]
		public void Traverse_InnerInstance()
		{
            TraverseNestedTypes instance = new TraverseNestedTypes(null);

            Traverse trv1 = Traverse.Create(instance);
            Traverse field1 = trv1.Field("innerInstance").Field("inner2").Field("field");
			field1.SetValue("somevalue");

            Traverse trv2 = Traverse.Create(instance);
            Traverse field2 = trv1.Field("innerInstance").Field("inner2").Field("field");
			Assert.AreEqual("somevalue", field2.GetValue());
		}

		[TestMethod]
		public void Traverse_InnerStatic()
		{
            Traverse trv1 = Traverse.Create(typeof(TraverseNestedTypes));
            Traverse field1 = trv1.Field("innerStatic").Field("inner2").Field("field");
			field1.SetValue("somevalue1");

            Traverse trv2 = Traverse.Create(typeof(TraverseNestedTypes));
            Traverse field2 = trv1.Field("innerStatic").Field("inner2").Field("field");
			Assert.AreEqual("somevalue1", field2.GetValue());

            TraverseNestedTypes _ = new TraverseNestedTypes("somevalue2");
            string value = Traverse
				.Create(typeof(TraverseNestedTypes))
				.Type("InnerStaticClass1")
				.Type("InnerStaticClass2")
				.Field("field")
				.GetValue<string>();
			Assert.AreEqual("somevalue2", value);
		}
	}
}