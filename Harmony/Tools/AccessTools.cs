﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Harmony
{
	public static class AccessTools
	{
		public static BindingFlags all = BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.Instance
			| BindingFlags.Static
			| BindingFlags.GetField
			| BindingFlags.SetField
			| BindingFlags.GetProperty
			| BindingFlags.SetProperty;

        public static Type TypeByName(string name) => 
            Type.GetType(name, false) ?? 
            AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .FirstOrDefault(x => x.FullName == name) ?? 
            AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .FirstOrDefault(x => x.Name == name);

        public static T FindRecursive<T>(Type type, Func<Type, T> action)
		{
			while (true)
			{
				T result = action(type);
				if (result != null) return result;
				if (type == typeof(object)) return default(T);
				type = type.BaseType;
			}
		}

		public static FieldInfo Field(Type type, string name)
		{
			if (type == null || name == null) return null;
			return FindRecursive(type, t => t.GetField(name, all));
		}

		public static PropertyInfo Property(Type type, string name)
		{
			if (type == null || name == null) return null;
			return FindRecursive(type, t => t.GetProperty(name, all));
		}

		public static MethodInfo Method(Type type, string name, Type[] parameters = null, Type[] generics = null)
		{
			if (type == null || name == null) return null;
			MethodInfo result;
			if (parameters == null)
				result = FindRecursive(type, t => t.GetMethod(name, all));
			else
			{
                ParameterModifier[] modifiers = new ParameterModifier[] { };
				result = FindRecursive(type, t => t.GetMethod(name, all, null, parameters, modifiers));
			}
			if (result == null) return null;
			if (generics != null) result = result.MakeGenericMethod(generics);
			return result;
		}

		public static ConstructorInfo Constructor(Type type, Type[] parameters = null)
		{
			if (type == null) return null;
			if (parameters == null) parameters = new Type[0];
			return FindRecursive(type, t => t.GetConstructor(all, null, parameters, new ParameterModifier[] { }));
		}

		public static Type GetReturnedType(MethodBase method)
		{
            if (method is ConstructorInfo constructor)
                return typeof(void);
            return ((MethodInfo)method).ReturnType;
		}

		public static Type Inner(Type type, string name)
		{
			if (type == null || name == null) return null;
			return FindRecursive(type, t => t.GetNestedType(name, all));
		}

		public static Type FirstInner(Type type, Func<Type, bool> predicate)
		{
			if (type == null || predicate == null) return null;
			return FindRecursive(type, t => t.GetNestedTypes(all).First(predicate));
		}

		public static Type[] GetTypes(object[] parameters)
		{
			if (parameters == null) return new Type[0];
			return parameters.Select(p => p == null ? typeof(object) : p.GetType()).ToArray();
		}

        public static List<string> GetFieldNames(Type type) => type.GetFields(all).Select(f => f.Name).ToList();

        public static List<string> GetFieldNames(object instance)
		{
			if (instance == null) return new List<string>();
			return GetFieldNames(instance.GetType());
		}

        public static List<string> GetPropertyNames(Type type) => type.GetProperties(all).Select(f => f.Name).ToList();

        public static List<string> GetPropertyNames(object instance)
		{
			if (instance == null) return new List<string>();
			return GetPropertyNames(instance.GetType());
		}

		public static object GetDefaultValue(Type type)
		{
			if (type == null) return null;
			if (type == typeof(void)) return null;
			if (type.IsValueType)
				return Activator.CreateInstance(type);
			return null;
		}

        public static bool IsStruct(Type type) => type.IsValueType && !IsValue(type) && !IsVoid(type);

        public static bool IsClass(Type type) => !type.IsValueType;

        public static bool IsValue(Type type) => type.IsPrimitive || type.IsEnum;

        public static bool IsVoid(Type type) => type == typeof(void);
    }
}