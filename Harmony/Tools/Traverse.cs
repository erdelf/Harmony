using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HarmonyErdelf
{
	public class Traverse
	{
		static AccessCache cache;

		Type _type;
		object _root;
		MemberInfo _info;
		MethodBase _method;
		object[] _params;

		[MethodImpl(MethodImplOptions.Synchronized)]
		static Traverse()
		{
			if (cache == null)
				cache = new AccessCache();
		}

        public static Traverse Create(Type type) => new Traverse(type);

        public static Traverse Create<T>() => Create(typeof(T));

        public static Traverse Create(object root) => new Traverse(root);

        public static Traverse CreateWithType(string name) => new Traverse(AccessTools.TypeByName(name));

        Traverse()
		{
		}

        public Traverse(Type type) => this._type = type;

        public Traverse(object root)
		{
            this._root = root;
            this._type = root?.GetType();
		}

		Traverse(object root, MemberInfo info, object[] index)
		{
            this._root = root;
            this._type = root?.GetType();
            this._info = info;
            this._params = index;
		}

		Traverse(object root, MethodInfo method, object[] parameter)
		{
            this._root = root;
            this._type = method.ReturnType;
            this._method = method;
            this._params = parameter;
		}

		public object GetValue()
		{
			if (this._info is FieldInfo)
				return ((FieldInfo) this._info).GetValue(this._root);
			if (this._info is PropertyInfo)
				return ((PropertyInfo) this._info).GetValue(this._root, AccessTools.all, null, this._params, CultureInfo.CurrentCulture);
			if (this._method != null)
				return this._method.Invoke(this._root, this._params);
			if (this._root == null && this._type != null) return this._type;
			return this._root;
		}

		public T GetValue<T>()
		{
            object value = GetValue();
			if (value == null) return default(T);
			return (T)value;
		}

		public object GetValue(params object[] arguments)
		{
			if (this._method == null)
				throw new Exception("cannot get method value without method");
			return this._method.Invoke(this._root, arguments);
		}

		public T GetValue<T>(params object[] arguments)
		{
			if (this._method == null)
				throw new Exception("cannot get method value without method");
			return (T) this._method.Invoke(this._root, arguments);
		}

		public Traverse SetValue(object value)
		{
			if (this._info is FieldInfo)
				((FieldInfo) this._info).SetValue(this._root, value, AccessTools.all, null, CultureInfo.CurrentCulture);
			if (this._info is PropertyInfo)
				((PropertyInfo) this._info).SetValue(this._root, value, AccessTools.all, null, this._params, CultureInfo.CurrentCulture);
			if (this._method != null)
				throw new Exception("cannot set value of a method");
			return this;
		}

		Traverse Resolve()
		{
			if (this._root == null && this._type != null) return this;
			return new Traverse(GetValue());
		}

		public Traverse Type(string name)
		{
			if (name == null) throw new Exception("name cannot be null");
			if (this._type == null) return new Traverse();
            Type type = AccessTools.Inner(this._type, name);
			if (type == null) return new Traverse();
			return new Traverse(type);
		}

		public Traverse Field(string name)
		{
			if (name == null) throw new Exception("name cannot be null");
            Traverse resolved = Resolve();
			if (resolved._type == null) return new Traverse();
            FieldInfo info = cache.GetFieldInfo(resolved._type, name);
			if (info == null) return new Traverse();
			if (info.IsStatic == false && resolved._root == null) return new Traverse();
			return new Traverse(resolved._root, info, null);
		}

		public Traverse Property(string name, object[] index = null)
		{
			if (name == null) throw new Exception("name cannot be null");
            Traverse resolved = Resolve();
			if (resolved._root == null || resolved._type == null) return new Traverse();
            PropertyInfo info = cache.GetPropertyInfo(resolved._type, name);
			if (info == null) return new Traverse();
			return new Traverse(resolved._root, info, index);
		}

		public Traverse Method(string name, params object[] arguments)
		{
			if (name == null) throw new Exception("name cannot be null");
            Traverse resolved = Resolve();
			if (resolved._type == null) return new Traverse();
            Type[] types = AccessTools.GetTypes(arguments);
            MethodBase method = cache.GetMethodInfo(resolved._type, name, types);
			if (method == null) return new Traverse();
			return new Traverse(resolved._root, (MethodInfo)method, arguments);
		}

		public Traverse Method(string name, Type[] paramTypes, object[] arguments = null)
		{
			if (name == null) throw new Exception("name cannot be null");
            Traverse resolved = Resolve();
			if (resolved._type == null) return new Traverse();
            MethodBase method = cache.GetMethodInfo(resolved._type, name, paramTypes);
			if (method == null) return new Traverse();
			return new Traverse(resolved._root, (MethodInfo)method, arguments);
		}

		public static void IterateFields(object source, Action<Traverse> action)
		{
            Traverse sourceTrv = Create(source);
			AccessTools.GetFieldNames(source).ForEach(f => action(sourceTrv.Field(f)));
		}

		public static void IterateFields(object source, object target, Action<Traverse, Traverse> action)
		{
            Traverse sourceTrv = Create(source);
            Traverse targetTrv = Create(target);
			AccessTools.GetFieldNames(source).ForEach(f => action(sourceTrv.Field(f), targetTrv.Field(f)));
		}

		public static void IterateProperties(object source, Action<Traverse> action)
		{
            Traverse sourceTrv = Create(source);
			AccessTools.GetPropertyNames(source).ForEach(f => action(sourceTrv.Property(f)));
		}

		public static void IterateProperties(object source, object target, Action<Traverse, Traverse> action)
		{
            Traverse sourceTrv = Create(source);
            Traverse targetTrv = Create(target);
			AccessTools.GetPropertyNames(source).ForEach(f => action(sourceTrv.Property(f), targetTrv.Property(f)));
		}

		public override string ToString()
		{
            object value = this._method ?? GetValue();
			if (value == null) return null;
			return value.ToString();
		}
	}
}