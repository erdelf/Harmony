using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarmonyTests.Assets
{
	public class TraverseTypes<T> where T : new()
	{
#pragma warning disable CS0414
		int intField;
		string stringField;
#pragma warning restore CS0414
		Type typeField;
		IEnumerable<bool> listOfBoolField;
		Dictionary<T, List<string>> mixedField;

		public T key;

		public TraverseTypes()
		{
            this.intField = 100;
            this.stringField = "hello";
            this.typeField = typeof(Console);
            this.listOfBoolField = (new bool[] { false, true }).Select(b => !b);

            Dictionary<T, List<string>> d = new Dictionary<T, List<string>>();
            List<string> l = new List<string>
            {
                "world"
            };
            this.key = new T();
			d.Add(this.key, l);
            this.mixedField = d;
		}
	}

	public class TraverseNestedTypes
	{
		private class InnerClass1
		{
			private class InnerClass2
			{
#pragma warning disable CS0414
				private string field;
#pragma warning restore CS0414

                public InnerClass2() => this.field = "helloInstance";
            }

			private InnerClass2 inner2;

            public InnerClass1() => this.inner2 = new InnerClass2();
        }

		private class InnerStaticFieldClass1
		{
			private class InnerStaticFieldClass2
			{
#pragma warning disable CS0414
				private static string field = "helloStatic";
#pragma warning restore CS0414
			}

			private static InnerStaticFieldClass2 inner2;

            public InnerStaticFieldClass1() => inner2 = new InnerStaticFieldClass2();
        }

		protected static class InnerStaticClass1
		{
			internal static class InnerStaticClass2
			{
				internal static string field;
			}
		}

		private InnerClass1 innerInstance;
		private static InnerStaticFieldClass1 innerStatic;

		public TraverseNestedTypes(string staticValue)
		{
            this.innerInstance = new InnerClass1();
			innerStatic = new InnerStaticFieldClass1();
			InnerStaticClass1.InnerStaticClass2.field = staticValue;
		}
	}

}