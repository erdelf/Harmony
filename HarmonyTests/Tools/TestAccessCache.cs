using Harmony;
using HarmonyTests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HarmonyTests
{
	[TestClass]
	public class Test_AccessCache
	{
		private void InjectField(AccessCache cache)
		{
            FieldInfo f_fields = cache.GetType().GetField("fields", AccessTools.all);
			Assert.IsNotNull(f_fields);
            Dictionary<Type, Dictionary<string, FieldInfo>> fields = (Dictionary<Type, Dictionary<string, FieldInfo>>)f_fields.GetValue(cache);
			Assert.IsNotNull(fields);
            fields.TryGetValue(typeof(AccessToolsClass), out Dictionary<string, FieldInfo> infos);
            Assert.IsNotNull(infos);

			infos.Remove("field");
			infos.Add("field", typeof(AccessToolsClass).GetField("field2", AccessTools.all));
		}

		private void InjectProperty(AccessCache cache)
		{
            FieldInfo f_properties = cache.GetType().GetField("properties", AccessTools.all);
			Assert.IsNotNull(f_properties);
            Dictionary<Type, Dictionary<string, PropertyInfo>> properties = (Dictionary<Type, Dictionary<string, PropertyInfo>>)f_properties.GetValue(cache);
			Assert.IsNotNull(properties);
            properties.TryGetValue(typeof(AccessToolsClass), out Dictionary<string, PropertyInfo> infos);
            Assert.IsNotNull(infos);

			infos.Remove("property");
			infos.Add("property", typeof(AccessToolsClass).GetProperty("property2", AccessTools.all));
		}

		private void InjectMethod(AccessCache cache)
		{
            FieldInfo f_methods = cache.GetType().GetField("methods", AccessTools.all);
			Assert.IsNotNull(f_methods);
            Dictionary<Type, Dictionary<string, Dictionary<Type[], MethodInfo>>> methods = (Dictionary<Type, Dictionary<string, Dictionary<Type[], MethodInfo>>>)f_methods.GetValue(cache);
			Assert.IsNotNull(methods);
            methods.TryGetValue(typeof(AccessToolsClass), out Dictionary<string, Dictionary<Type[], MethodInfo>> dicts);
            Assert.IsNotNull(dicts);
            dicts.TryGetValue("method", out Dictionary<Type[], MethodInfo> infos);
            Assert.IsNotNull(dicts);

			infos.Remove(Type.EmptyTypes);
			infos.Add(Type.EmptyTypes, typeof(AccessToolsClass).GetMethod("method2", AccessTools.all));
		}

		[TestMethod]
		public void AccessCache_Field()
		{
            Type type = typeof(AccessToolsClass);

			Assert.IsNotNull((new AccessCache()).GetFieldInfo(type, "field"));

            AccessCache cache1 = new AccessCache();
            FieldInfo finfo1 = cache1.GetFieldInfo(type, "field");
			InjectField(cache1);
            AccessCache cache2 = new AccessCache();
            FieldInfo finfo2 = cache2.GetFieldInfo(type, "field");
			Assert.AreSame(finfo1, finfo2);

            AccessCache cache = new AccessCache();
            FieldInfo finfo3 = cache.GetFieldInfo(type, "field");
			InjectField(cache);
            FieldInfo finfo4 = cache.GetFieldInfo(type, "field");
			Assert.AreNotSame(finfo3, finfo4);
		}

		[TestMethod]
		public void AccessCache_Property()
		{
            Type type = typeof(AccessToolsClass);

			Assert.IsNotNull((new AccessCache()).GetPropertyInfo(type, "property"));

            AccessCache cache1 = new AccessCache();
            PropertyInfo pinfo1 = cache1.GetPropertyInfo(type, "property");
			InjectProperty(cache1);
            AccessCache cache2 = new AccessCache();
            PropertyInfo pinfo2 = cache2.GetPropertyInfo(type, "property");
			Assert.AreSame(pinfo1, pinfo2);

            AccessCache cache = new AccessCache();
            PropertyInfo pinfo3 = cache.GetPropertyInfo(type, "property");
			InjectProperty(cache);
            PropertyInfo pinfo4 = cache.GetPropertyInfo(type, "property");
			Assert.AreNotSame(pinfo3, pinfo4);
		}

		[TestMethod]
		public void AccessCache_Method()
		{
            Type type = typeof(AccessToolsClass);

			Assert.IsNotNull((new AccessCache()).GetMethodInfo(type, "method", Type.EmptyTypes));

            AccessCache cache1 = new AccessCache();
            MethodBase minfo1 = cache1.GetMethodInfo(type, "method", Type.EmptyTypes);
			InjectMethod(cache1);
            AccessCache cache2 = new AccessCache();
            MethodBase minfo2 = cache2.GetMethodInfo(type, "method", Type.EmptyTypes);
			Assert.AreSame(minfo1, minfo2);

            AccessCache cache = new AccessCache();
            MethodBase minfo3 = cache.GetMethodInfo(type, "method", Type.EmptyTypes);
			InjectMethod(cache);
            MethodBase minfo4 = cache.GetMethodInfo(type, "method", Type.EmptyTypes);
			Assert.AreNotSame(minfo3, minfo4);
		}
	}
}