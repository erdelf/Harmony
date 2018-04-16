using System;
using System.Collections.Generic;
using System.Reflection;

namespace HarmonyErdelf
{
    public class AccessCache
	{
		Dictionary<Type, Dictionary<string, FieldInfo>> fields = new Dictionary<Type, Dictionary<string, FieldInfo>>();
		Dictionary<Type, Dictionary<string, PropertyInfo>> properties = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
		readonly Dictionary<Type, Dictionary<string, Dictionary<int, MethodBase>>> methods = new Dictionary<Type, Dictionary<string, Dictionary<int, MethodBase>>>();

		public FieldInfo GetFieldInfo(Type type, string name)
		{
            this.fields.TryGetValue(type, out Dictionary<string, FieldInfo> fieldsByType);
            if (fieldsByType == null)
			{
				fieldsByType = new Dictionary<string, FieldInfo>();
                this.fields.Add(type, fieldsByType);
			}

            fieldsByType.TryGetValue(name, out FieldInfo field);
            if (field == null)
			{
				field = AccessTools.Field(type, name);
				fieldsByType.Add(name, field);
			}
			return field;
		}

		public PropertyInfo GetPropertyInfo(Type type, string name)
		{
            this.properties.TryGetValue(type, out Dictionary<string, PropertyInfo> propertiesByType);
            if (propertiesByType == null)
			{
				propertiesByType = new Dictionary<string, PropertyInfo>();
                this.properties.Add(type, propertiesByType);
			}

            propertiesByType.TryGetValue(name, out PropertyInfo property);
            if (property == null)
			{
				property = AccessTools.Property(type, name);
				propertiesByType.Add(name, property);
			}
			return property;
		}

		static int CombinedHashCode(IEnumerable<object> objects)
		{
			int hash1 = (5381 << 16) + 5381;
			int hash2 = hash1;
			int i = 0;
			foreach (object obj in objects)
			{
				if (i % 2 == 0)
					hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ obj.GetHashCode();
				else
					hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ obj.GetHashCode();
				++i;
			}
			return hash1 + (hash2 * 1566083941);
		}

		public MethodBase GetMethodInfo(Type type, string name, Type[] arguments)
		{
            this.methods.TryGetValue(type, out Dictionary<string, Dictionary<int, MethodBase>> methodsByName);
            if (methodsByName == null)
			{
				methodsByName = new Dictionary<string, Dictionary<int, MethodBase>>();
                this.methods.Add(type, methodsByName);
			}

            methodsByName.TryGetValue(name, out Dictionary<int, MethodBase> methodsByArguments);
            if (methodsByArguments == null)
			{
				methodsByArguments = new Dictionary<int, MethodBase>();
				methodsByName.Add(name, methodsByArguments);
			}

            int argumentsHash = CombinedHashCode(arguments);
            methodsByArguments.TryGetValue(argumentsHash, out MethodBase method);
			if (method == null)
			{
				method = AccessTools.Method(type, name, arguments);
				methodsByArguments.Add(argumentsHash, method);
			}

			return method;
		}
	}
}