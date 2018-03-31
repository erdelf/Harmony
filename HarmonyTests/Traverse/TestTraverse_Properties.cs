using Harmony;
using HarmonyTests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HarmonyTests
{
	[TestClass]
	public class TestTraverse_Properties
	{
		// Traverse.ToString() should return the value of a traversed property
		//
		[TestMethod]
		public void Traverse_Property_ToString()
		{
            TraverseProperties_AccessModifiers instance = new TraverseProperties_AccessModifiers(TraverseProperties.testStrings);

            Traverse trv = Traverse.Create(instance).Property(TraverseProperties.propertyNames[0]);
			Assert.AreEqual(TraverseProperties.testStrings[0], trv.ToString());
		}

		// Traverse.GetValue() should return the value of a traversed property
		// regardless of its access modifier
		//
		[TestMethod]
		public void Traverse_Property_GetValue()
		{
            TraverseProperties_AccessModifiers instance = new TraverseProperties_AccessModifiers(TraverseProperties.testStrings);
            Traverse trv = Traverse.Create(instance);

			for (int i = 0; i < TraverseProperties.testStrings.Length; i++)
			{
                string name = TraverseProperties.propertyNames[i];
                Traverse ptrv = trv.Property(name);
				Assert.IsNotNull(ptrv);

				Assert.AreEqual(TraverseProperties.testStrings[i], ptrv.GetValue());
				Assert.AreEqual(TraverseProperties.testStrings[i], ptrv.GetValue<string>());
			}
		}

		// Traverse.SetValue() should set the value of a traversed property
		// regardless of its access modifier
		//
		[TestMethod]
		public void Traverse_Property_SetValue()
		{
            TraverseProperties_AccessModifiers instance = new TraverseProperties_AccessModifiers(TraverseProperties.testStrings);
            Traverse trv = Traverse.Create(instance);

			for (int i = 0; i < TraverseProperties.testStrings.Length - 1; i++)
			{
                string newValue = "newvalue" + i;

				// before
				Assert.AreEqual(TraverseProperties.testStrings[i], instance.GetTestProperty(i));

                string name = TraverseProperties.propertyNames[i];
                Traverse ptrv = trv.Property(name);
				ptrv.SetValue(newValue);

				// after
				Assert.AreEqual(newValue, instance.GetTestProperty(i));
				Assert.AreEqual(newValue, ptrv.GetValue());
				Assert.AreEqual(newValue, ptrv.GetValue<string>());
			}
		}
	}
}