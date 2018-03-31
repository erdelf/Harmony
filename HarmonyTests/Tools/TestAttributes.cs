using Microsoft.VisualStudio.TestTools.UnitTesting;
using Harmony;
using HarmonyTests.Assets;
using System.Linq;

namespace HarmonyTests.Tools
{
	[TestClass]
	public class Test_Attributes
	{
		[TestMethod]
		public void TestAttributes()
		{
            System.Type type = typeof(AllAttributesClass);
            System.Collections.Generic.List<HarmonyMethod> infos = type.GetHarmonyMethods();
            HarmonyMethod info = HarmonyMethod.Merge(infos);
			Assert.IsNotNull(info);
			Assert.AreEqual(typeof(string), info.originalType);
			Assert.AreEqual("foobar", info.methodName);
			Assert.IsNotNull(info.parameter);
			Assert.AreEqual(2, info.parameter.Length);
			Assert.AreEqual(typeof(float), info.parameter[0]);
			Assert.AreEqual(typeof(string), info.parameter[1]);
		}
	}
}