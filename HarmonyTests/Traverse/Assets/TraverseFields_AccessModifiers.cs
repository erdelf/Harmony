namespace HarmonyTests.Assets
{
	public class TraverseFields
	{
		public static string[] testStrings = new string[] { "test01", "test02", "test03", "test04" };
		public static string[] fieldNames = new string[] { "publicField", "privateField", "protectedField", "internalField" };
	}

	public class TraverseFields_AccessModifiers
	{
		public string publicField;
		private string privateField;
		protected string protectedField;
		internal string internalField;

		public TraverseFields_AccessModifiers(string[] s)
		{
            this.publicField = s[0];
            this.privateField = s[1];
            this.protectedField = s[2];
            this.internalField = s[3];
		}

		public string GetTestField(int n)
		{
			switch (n)
			{
				case 0:
					return this.publicField;
				case 1:
					return this.privateField;
				case 2:
					return this.protectedField;
				case 3:
					return this.internalField;
			}
			return null;
		}

        public override string ToString() => "TraverseFields_AccessModifiers";
    }
}