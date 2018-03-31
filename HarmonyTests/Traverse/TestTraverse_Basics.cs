﻿using Harmony;
using HarmonyTests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HarmonyTests
{
	[TestClass]
	public class TestTraverse_Basics
	{
		// Basic integrity check for our test class and the field-testvalue relations
		//
		[TestMethod]
		public void Instantiate_TraverseFields_AccessModifiers()
		{
            TraverseFields_AccessModifiers instance = new TraverseFields_AccessModifiers(TraverseFields.testStrings);

			for (int i = 0; i < TraverseFields.testStrings.Length; i++)
				Assert.AreEqual(TraverseFields.testStrings[i], instance.GetTestField(i));
		}

		public static void AssertIsEmpty(Traverse trv)
		{
			Assert.AreEqual(null, AccessTools.Field(typeof(Traverse), "_root").GetValue(trv));
			Assert.AreEqual(null, AccessTools.Field(typeof(Traverse), "_type").GetValue(trv));
			Assert.AreEqual(null, AccessTools.Field(typeof(Traverse), "_info").GetValue(trv));
			Assert.AreEqual(null, AccessTools.Field(typeof(Traverse), "_index").GetValue(trv));
		}

		class FooBar
		{
#pragma warning disable CS0169
			string field;
#pragma warning restore CS0169
		}

		// Traverse should default to an empty instance to avoid errors
		//
		[TestMethod]
		public void Traverse_SilentFailures()
		{
            Traverse trv1 = new Traverse(null);
			AssertIsEmpty(trv1);

			trv1 = Traverse.Create(null);
			AssertIsEmpty(trv1);

            Traverse trv2 = trv1.Type("FooBar");
			AssertIsEmpty(trv2);

            Traverse trv3 = Traverse.Create<FooBar>().Field("field");
			AssertIsEmpty(trv3);

            Traverse trv4 = new Traverse(new FooBar()).Field("field");
			AssertIsEmpty(trv4.Method("", new object[0]));
			AssertIsEmpty(trv4.Method("", new Type[0], new object[0]));
		}

		// Traverse should handle basic null values
		//
		[TestMethod]
		public void Traverse_Create_With_Null()
		{
            Traverse trv = Traverse.Create(null);

			Assert.IsNotNull(trv);
			Assert.IsNull(trv.ToString());

            // field access

            Traverse ftrv = trv.Field("foo");
			Assert.IsNotNull(ftrv);

			Assert.IsNull(ftrv.GetValue());
			Assert.IsNull(ftrv.ToString());
			Assert.AreEqual(0, ftrv.GetValue<int>());
			Assert.AreSame(ftrv, ftrv.SetValue(123));

            // property access

            Traverse ptrv = trv.Property("foo");
			Assert.IsNotNull(ptrv);

			Assert.IsNull(ptrv.GetValue());
			Assert.IsNull(ptrv.ToString());
			Assert.IsNull(ptrv.GetValue<string>());
			Assert.AreSame(ptrv, ptrv.SetValue("test"));

            // method access

            Traverse mtrv = trv.Method("zee");
			Assert.IsNotNull(mtrv);

			Assert.IsNull(mtrv.GetValue());
			Assert.IsNull(mtrv.ToString());
			Assert.AreEqual(0, mtrv.GetValue<float>());
			Assert.AreSame(mtrv, mtrv.SetValue(null));
		}

		// Traverse.ToString() should return a meaningful string representation of its initial value
		//
		[TestMethod]
		public void Traverse_Create_Instance_ToString()
		{
            TraverseFields_AccessModifiers instance = new TraverseFields_AccessModifiers(TraverseFields.testStrings);

            Traverse trv = Traverse.Create(instance);
			Assert.AreEqual(instance.ToString(), trv.ToString());
		}

		// Traverse.ToString() should return a meaningful string representation of its initial type
		//
		[TestMethod]
		public void Traverse_Create_Type_ToString()
		{
            TraverseFields_AccessModifiers instance = new TraverseFields_AccessModifiers(TraverseFields.testStrings);

            Type type = typeof(TraverseFields_AccessModifiers);
            Traverse trv = Traverse.Create(type);
			Assert.AreEqual(type.ToString(), trv.ToString());
		}
	}
}
