using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HarmonyTests.Assets
{
	public class TraverseProperties
	{
		public static string[] testStrings = new string[] { "test01", "test02", "test03", "test04", "test05", "test06", "test07" };
		public static string[] propertyNames = new string[] { "publicProperty", "publicPrivateProperty", "autoProperty", "baseProperty1", "baseProperty2", "baseProperty3", "immediateProperty" };
	}

	public class TraverseProperties_BaseClass
	{
		string _basePropertyField1;
		protected virtual string BaseProperty1
        {
            get => this._basePropertyField1;
            set => this._basePropertyField1 = value;
        }

        string _basePropertyField2;
		protected virtual string BaseProperty2
        {
            get => this._basePropertyField2;
            set => this._basePropertyField2 = value;
        }

        public string BaseProperty3
        {
            get => throw new Exception();
            set => throw new Exception();
        }
    }

	public class TraverseProperties_AccessModifiers : TraverseProperties_BaseClass
	{
		private string _publicPropertyField;
		public string PublicProperty
        {
            get => this._publicPropertyField;
            set => this._publicPropertyField = value;
        }

        private string _publicPrivatePropertyField;
		public string PublicPrivateProperty
        {
            get => this._publicPrivatePropertyField;
            private set => this._publicPrivatePropertyField = value;
        }

        string AutoProperty { get; set; }

		protected override string BaseProperty1
        {
            get => base.BaseProperty1;
            set => base.BaseProperty1 = value;
        }

        // baseProperty2 defined and used in base class

        string _basePropertyField3;
		public new string BaseProperty3
        {
            get => this._basePropertyField3;
            set => this._basePropertyField3 = value;
        }

        string ImmediateProperty => TraverseProperties.testStrings.Last();

		public TraverseProperties_AccessModifiers(string[] s)
		{
            this.PublicProperty = s[0];
            this.PublicPrivateProperty = s[1];
            this.AutoProperty = s[2];
            this.BaseProperty1 = s[3];
            this.BaseProperty2 = s[4];
            this.BaseProperty3 = s[5];
			// immediateProperty is readonly
		}

		public string GetTestProperty(int n)
		{
			switch (n)
			{
				case 0:
					return this.PublicProperty;
				case 1:
					return this.PublicPrivateProperty;
				case 2:
					return this.AutoProperty;
				case 3:
					return this.BaseProperty1;
				case 4:
					return this.BaseProperty2;
				case 5:
					return this.BaseProperty3;
				case 6:
					return this.ImmediateProperty;
			}
			return null;
		}

		public void SetTestProperty(int n, string value)
		{
			switch (n)
			{
				case 0:
                    this.PublicProperty = value;
					break;
				case 1:
                    this.PublicPrivateProperty = value;
					break;
				case 2:
                    this.AutoProperty = value;
					break;
				case 3:
                    this.BaseProperty1 = value;
					break;
				case 4:
                    this.BaseProperty2 = value;
					break;
				case 5:
                    this.BaseProperty3 = value;
					break;
				case 6:
					// immediateProperty = value;
					break;
			}
		}

        public override string ToString() => "TraverseProperties_AccessModifiers";
    }
}