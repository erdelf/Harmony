using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Harmony
{
	public static class GeneralExtensions
	{
		public static string Description(this Type[] parameters)
		{
            IEnumerable<string> types = parameters.Select(p => p == null ? "null" : p.FullName);
			return "(" + types.Aggregate("", (s, x) => s.Length == 0 ? x : s + ", " + x) + ")";
		}

        public static Type[] Types(this ParameterInfo[] pinfo) => pinfo.Select(pi => pi.ParameterType).ToArray();

        public static T GetValueSafe<S, T>(this Dictionary<S, T> dictionary, S key)
		{
            if (dictionary.TryGetValue(key, out T result))
                return result;
            return default(T);
		}

		public static T GetTypedValue<T>(this Dictionary<string, object> dictionary, string key)
		{
            if (dictionary.TryGetValue(key, out object result))
                if (result is T)
                    return (T) result;
            return default(T);
		}
	}

	public static class CollectionExtensions
	{
		public static IEnumerable<T> Do<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence == null) return null;
            IEnumerator<T> enumerator = sequence.GetEnumerator();
			while (enumerator.MoveNext()) action(enumerator.Current);
			return sequence;
		}

        public static IEnumerable<T> DoIf<T>(this IEnumerable<T> sequence, Func<T, bool> condition, Action<T> action) => sequence.Where(condition).Do(action);

        public static IEnumerable<T> Add<T>(this IEnumerable<T> sequence, T item) => (sequence ?? Enumerable.Empty<T>()).Concat(new[] { item });

        public static T[] AddRangeToArray<T>(this T[] sequence, T[] items) => (sequence ?? Enumerable.Empty<T>()).Concat(items).ToArray();

        public static T[] AddToArray<T>(this T[] sequence, T item) => Add(sequence, item).ToArray();
    }
}