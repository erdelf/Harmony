using System;

namespace Harmony
{
	public enum PropertyMethod
	{
#pragma warning disable IDE1006 // Benennungsstile
        Getter,
		Setter
#pragma warning restore IDE1006 // Benennungsstile
    }

    public class HarmonyAttribute : Attribute
	{
		public HarmonyMethod info = new HarmonyMethod();
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class HarmonyPatch : HarmonyAttribute
	{
		public HarmonyPatch()
		{
		}

        public HarmonyPatch(Type type) => this.info.originalType = type;

        public HarmonyPatch(string methodName) => this.info.methodName = methodName;

        public HarmonyPatch(string propertyName, PropertyMethod type)
		{
            string prefix = type == PropertyMethod.Getter ? "get_" : "set_";
            this.info.methodName = prefix + propertyName;
		}

        public HarmonyPatch(Type[] parameter) => this.info.parameter = parameter;

        public HarmonyPatch(Type type, string methodName, Type[] parameter = null)
		{
            this.info.originalType = type;
            this.info.methodName = methodName;
            this.info.parameter = parameter;
		}

		public HarmonyPatch(Type type, Type[] parameter = null)
		{
            this.info.originalType = type;
            this.info.parameter = parameter;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HarmonyPriority : HarmonyAttribute
	{
        public HarmonyPriority(int prioritiy) => this.info.prioritiy = prioritiy;
    }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HarmonyBefore : HarmonyAttribute
	{
        public HarmonyBefore(params string[] before) => this.info.before = before;
    }

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HarmonyAfter : HarmonyAttribute
	{
        public HarmonyAfter(params string[] after) => this.info.after = after;
    }

	// If you don't want to use the special method names you can annotate
	// using the following attributes:

	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyPrepare : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyTargetMethod : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyPrefix : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyPostfix : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyTranspiler : Attribute
	{
	}

	// If you want to rename parameter name in case it's obfuscated or something else 
	// you can use the following attribute:

	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class HarmonyParameter : Attribute
	{
		public string OriginalName { get; private set; }
		public string NewName { get; private set; }

		public HarmonyParameter(string originalName) : this(originalName, null)
		{
		}

		public HarmonyParameter(string originalName, string newName)
		{
            this.OriginalName = originalName;
            this.NewName = newName;
		}
	}
}