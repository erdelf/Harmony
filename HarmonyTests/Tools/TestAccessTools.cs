﻿using Harmony;
using HarmonyTests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HarmonyTests
{
	[TestClass]
	public class Test_AccessTools
	{
		[TestMethod]
		public void AccessTools_Field()
		{
            Type type = typeof(AccessToolsClass);

			Assert.IsNull(AccessTools.Field(null, null));
			Assert.IsNull(AccessTools.Field(type, null));
			Assert.IsNull(AccessTools.Field(null, "field"));
			Assert.IsNull(AccessTools.Field(type, "unknown"));

            FieldInfo field = AccessTools.Field(type, "field");
			Assert.IsNotNull(field);
			Assert.AreEqual(type, field.DeclaringType);
			Assert.AreEqual("field", field.Name);
		}

		[TestMethod]
		public void AccessTools_Property()
		{
            Type type = typeof(AccessToolsClass);

			Assert.IsNull(AccessTools.Property(null, null));
			Assert.IsNull(AccessTools.Property(type, null));
			Assert.IsNull(AccessTools.Property(null, "property"));
			Assert.IsNull(AccessTools.Property(type, "unknown"));

            PropertyInfo prop = AccessTools.Property(type, "property");
			Assert.IsNotNull(prop);
			Assert.AreEqual(type, prop.DeclaringType);
			Assert.AreEqual("property", prop.Name);
		}

		[TestMethod]
		public void AccessTools_Method()
		{
            Type type = typeof(AccessToolsClass);

			Assert.IsNull(AccessTools.Method(null, null));
			Assert.IsNull(AccessTools.Method(type, null));
			Assert.IsNull(AccessTools.Method(null, "method"));
			Assert.IsNull(AccessTools.Method(type, "unknown"));

            MethodInfo m1 = AccessTools.Method(type, "method");
			Assert.IsNotNull(m1);
			Assert.AreEqual(type, m1.DeclaringType);
			Assert.AreEqual("method", m1.Name);

            MethodInfo m2 = AccessTools.Method(type, "method", new Type[] { });
			Assert.IsNotNull(m2);

            MethodInfo m3 = AccessTools.Method(type, "setfield", new Type[] { typeof(string) });
			Assert.IsNotNull(m3);
		}

		[TestMethod]
		public void AccessTools_InnerClass()
		{
            Type type = typeof(AccessToolsClass);

			Assert.IsNull(AccessTools.Inner(null, null));
			Assert.IsNull(AccessTools.Inner(type, null));
			Assert.IsNull(AccessTools.Inner(null, "inner"));
			Assert.IsNull(AccessTools.Inner(type, "unknown"));

            Type cls = AccessTools.Inner(type, "inner");
			Assert.IsNotNull(cls);
			Assert.AreEqual(type, cls.DeclaringType);
			Assert.AreEqual("inner", cls.Name);
		}

		[TestMethod]
		public void AccessTools_GetTypes()
		{
            Type[] empty = AccessTools.GetTypes(null);
			Assert.IsNotNull(empty);
			Assert.AreEqual(0, empty.Length);

            // TODO: typeof(null) is ambiguous and resolves for now to <object>. is this a problem?
            Type[] types = AccessTools.GetTypes(new object[] { "hi", 123, null, new Test_AccessTools() });
			Assert.IsNotNull(types);
			Assert.AreEqual(4, types.Length);
			Assert.AreEqual(typeof(string), types[0]);
			Assert.AreEqual(typeof(int), types[1]);
			Assert.AreEqual(typeof(object), types[2]);
			Assert.AreEqual(typeof(Test_AccessTools), types[3]);
		}

		[TestMethod]
		public void AccessTools_GetDefaultValue()
		{
			Assert.AreEqual(null, AccessTools.GetDefaultValue(null));
			Assert.AreEqual((float)0, AccessTools.GetDefaultValue(typeof(float)));
			Assert.AreEqual(null, AccessTools.GetDefaultValue(typeof(string)));
			Assert.AreEqual(BindingFlags.Default, AccessTools.GetDefaultValue(typeof(BindingFlags)));
			Assert.AreEqual(null, AccessTools.GetDefaultValue(typeof(IEnumerable<bool>)));
			Assert.AreEqual(null, AccessTools.GetDefaultValue(typeof(void)));
		}

		[TestMethod]
		public void AccessTools_TypeExtension_Description()
		{
            Type[] types = new Type[] { typeof(string), typeof(int), null, typeof(void), typeof(Test_AccessTools) };
			Assert.AreEqual("(System.String, System.Int32, null, System.Void, HarmonyTests.Test_AccessTools)", types.Description());
		}

		[TestMethod]
		public void AccessTools_TypeExtension_Types()
		{
            // public static void Resize<T>(ref T[] array, int newSize);
            MethodInfo method = typeof(Array).GetMethod("Resize");
            ParameterInfo[] pinfo = method.GetParameters();
            Type[] types = pinfo.Types();

			Assert.IsNotNull(types);
			Assert.AreEqual(2, types.Length);
			Assert.AreEqual(pinfo[0].ParameterType, types[0]);
			Assert.AreEqual(pinfo[1].ParameterType, types[1]);
		}
	}
}