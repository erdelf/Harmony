namespace HarmonyTests.Assets
{
	public class AccessToolsClass
	{
		private class Inner
		{
		}

		private string field;
		private string field2;

		private int _property;
		private int Property
        {
            get => this._property;
            set => this._property = value;
        }
        private int Property2
        {
            get => this._property;
            set => this._property = value;
        }

        public AccessToolsClass()
		{
            this.field = "hello";
            this.field2 = "dummy";
		}

        public string Method() => this.field;

        public string Method2() => this.field2;

        public void Setfield(string val) => this.field = val;
    }
}