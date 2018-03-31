using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Harmony
{
	public static class PatchInfoSerialization
	{
		class Binder : SerializationBinder
		{
			public override Type BindToType(string assemblyName, string typeName)
			{
                Type[] types = new Type[] {
					typeof(PatchInfo),
					typeof(Patch[]),
					typeof(Patch)
				};
				foreach (Type type in types)
					if (typeName == type.FullName)
						return type;
                Type typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
				return typeToDeserialize;
			}
		}

		public static byte[] Serialize(this PatchInfo patchInfo)
		{
#pragma warning disable XS0001
			using (MemoryStream streamMemory = new MemoryStream())
			{
                BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(streamMemory, patchInfo);
				return streamMemory.GetBuffer();
			}
#pragma warning restore XS0001
		}

		public static PatchInfo Deserialize(byte[] bytes)
		{
            BinaryFormatter formatter = new BinaryFormatter
            {
                Binder = new Binder()
            };
#pragma warning disable XS0001
            MemoryStream streamMemory = new MemoryStream(bytes);
#pragma warning restore XS0001
			return (PatchInfo)formatter.Deserialize(streamMemory);
		}

		// general sorting by (in that order): before, after, priority and index
		public static int PriorityComparer(object obj, int index, int priority, string[] before, string[] after)
		{
            Traverse trv = Traverse.Create(obj);
            string theirOwner = trv.Field("owner").GetValue<string>();
            int theirPriority = trv.Field("priority").GetValue<int>();
            int theirIndex = trv.Field("index").GetValue<int>();

			if (before != null && Array.IndexOf(before, theirOwner) > -1)
				return -1;
			if (after != null && Array.IndexOf(after, theirOwner) > -1)
				return 1;

			if (priority != theirPriority)
				return -(priority.CompareTo(theirPriority));

			return index.CompareTo(theirIndex);
		}
	}

	[Serializable]
	public class PatchInfo
	{
		public Patch[] prefixes;
		public Patch[] postfixes;
		public Patch[] transpilers;

		public PatchInfo()
		{
            this.prefixes = new Patch[0];
            this.postfixes = new Patch[0];
            this.transpilers = new Patch[0];
		}

		public void AddPrefix(MethodInfo patch, string owner, int priority, string[] before, string[] after)
		{
            System.Collections.Generic.List<Patch> l = this.prefixes.ToList();
			l.Add(new Patch(patch, this.prefixes.Count() + 1, owner, priority, before, after));
            this.prefixes = l.ToArray();
		}

		public void AddPostfix(MethodInfo patch, string owner, int priority, string[] before, string[] after)
		{
            System.Collections.Generic.List<Patch> l = this.postfixes.ToList();
			l.Add(new Patch(patch, this.postfixes.Count() + 1, owner, priority, before, after));
            this.postfixes = l.ToArray();
		}

		public void AddTranspiler(MethodInfo patch, string owner, int priority, string[] before, string[] after)
		{
            System.Collections.Generic.List<Patch> l = this.transpilers.ToList();
			l.Add(new Patch(patch, this.transpilers.Count() + 1, owner, priority, before, after));
            this.transpilers = l.ToArray();
		}
	}

	[Serializable]
	public class Patch : IComparable
	{
		readonly public int index;
		readonly public string owner;
		readonly public int priority;
		readonly public string[] before;
		readonly public string[] after;

		readonly public MethodInfo patch;

		public Patch(MethodInfo patch, int index, string owner, int priority, string[] before, string[] after)
		{
			if (patch is DynamicMethod) throw new Exception("Cannot directly reference dynamic method \"" + patch + "\" in Harmony. Use a factory method instead that will return the dynamic method.");

			this.index = index;
			this.owner = owner;
			this.priority = priority;
			this.before = before;
			this.after = after;
			this.patch = patch;
		}

		public MethodInfo GetMethod(MethodBase original)
		{
			if (this.patch.ReturnType != typeof(DynamicMethod)) return this.patch;
			if (this.patch.IsStatic == false) return this.patch;
            ParameterInfo[] parameters = this.patch.GetParameters();
			if (parameters.Count() != 1) return this.patch;
			if (parameters[0].ParameterType != typeof(MethodBase)) return this.patch;

			// we have a DynamicMethod factory, let's use it
			return this.patch.Invoke(null, new object[] { original }) as DynamicMethod;
		}

        public override bool Equals(object obj) => ((obj != null) && (obj is Patch) && (this.patch == ((Patch) obj).patch));

        public int CompareTo(object obj) => PatchInfoSerialization.PriorityComparer(obj, this.index, this.priority, this.before, this.after);

        public override int GetHashCode() => this.patch.GetHashCode();
    }
}