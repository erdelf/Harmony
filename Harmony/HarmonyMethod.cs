using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Harmony
{
	public class HarmonyMethod
	{
		public MethodInfo method; // need to be called 'method'

		public Type originalType;
		public string methodName;
		public Type[] parameter;
		public int prioritiy = -1;
		public string[] before;
		public string[] after;

		public HarmonyMethod()
		{
		}

		void ImportMethod(MethodInfo theMethod)
		{
            this.method = theMethod;
			if (this.method != null)
			{
                List<HarmonyMethod> infos = this.method.GetHarmonyMethods();
				if (infos != null)
					Merge(infos).CopyTo(this);
			}
		}

        public HarmonyMethod(MethodInfo method) => ImportMethod(method);

        public HarmonyMethod(Type type, string name, Type[] parameters = null)
		{
            MethodInfo method = AccessTools.Method(type, name, parameters);
			ImportMethod(method);
		}

        public static List<string> HarmonyFields() => AccessTools
                .GetFieldNames(typeof(HarmonyMethod))
                .Where(s => s != "method")
                .ToList();

        public static HarmonyMethod Merge(List<HarmonyMethod> attributes)
		{
            HarmonyMethod result = new HarmonyMethod();
			if (attributes == null) return result;
            Traverse resultTrv = Traverse.Create(result);
			attributes.ForEach(attribute =>
			{
                Traverse trv = Traverse.Create(attribute);
				HarmonyFields().ForEach(f =>
				{
                    object val = trv.Field(f).GetValue();
					if (val != null)
						resultTrv.Field(f).SetValue(val);
				});
			});
			return result;
		}
	}

	public static class HarmonyMethodExtensions
	{
		public static void CopyTo(this HarmonyMethod from, HarmonyMethod to)
		{
			if (to == null) return;
            Traverse fromTrv = Traverse.Create(from);
            Traverse toTrv = Traverse.Create(to);
			HarmonyMethod.HarmonyFields().ForEach(f =>
			{
                object val = fromTrv.Field(f).GetValue();
				if (val != null) toTrv.Field(f).SetValue(val);
			});
		}

		public static HarmonyMethod Clone(this HarmonyMethod original)
		{
            HarmonyMethod result = new HarmonyMethod();
			original.CopyTo(result);
			return result;
		}

		public static HarmonyMethod Merge(this HarmonyMethod master, HarmonyMethod detail)
		{
			if (detail == null) return master;
            HarmonyMethod result = new HarmonyMethod();
            Traverse resultTrv = Traverse.Create(result);
            Traverse masterTrv = Traverse.Create(master);
            Traverse detailTrv = Traverse.Create(detail);
			HarmonyMethod.HarmonyFields().ForEach(f =>
			{
                object baseValue = masterTrv.Field(f).GetValue();
                object detailValue = detailTrv.Field(f).GetValue();
				resultTrv.Field(f).SetValue(detailValue ?? baseValue);
			});
			return result;
		}

        public static List<HarmonyMethod> GetHarmonyMethods(this Type type) => type.GetCustomAttributes(true)
                        .Where(attr => attr is HarmonyAttribute)
                        .Cast<HarmonyAttribute>()
                        .Select(attr => attr.info)
                        .ToList();

        public static List<HarmonyMethod> GetHarmonyMethods(this MethodBase method)
		{
			if (method is DynamicMethod) return new List<HarmonyMethod>();
			return method.GetCustomAttributes(true)
						.Where(attr => attr is HarmonyAttribute)
						.Cast<HarmonyAttribute>()
						.Select(attr => attr.info)
						.ToList();
		}
	}
}