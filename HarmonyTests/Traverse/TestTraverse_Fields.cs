using Harmony;
using HarmonyTests.Assets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HarmonyTests
{
	[TestClass]
	public class TestTraverse_Fields
	{
		// Traverse.ToString() should return the value of a traversed field
		//
		[TestMethod]
		public void Traverse_Field_ToString()
		{
            TraverseFields_AccessModifiers instance = new TraverseFields_AccessModifiers(TraverseFields.testStrings);

            Traverse trv = Traverse.Create(instance).Field(TraverseFields.fieldNames[0]);
			Assert.AreEqual(TraverseFields.testStrings[0], trv.ToString());
		}

		// Traverse.GetValue() should return the value of a traversed field
		// regardless of its access modifier
		//
		[TestMethod]
		public void Traverse_Field_GetValue()
		{
            TraverseFields_AccessModifiers instance = new TraverseFields_AccessModifiers(TraverseFields.testStrings);
            Traverse trv = Traverse.Create(instance);

			for (int i = 0; i < TraverseFields.testStrings.Length; i++)
			{
                string name = TraverseFields.fieldNames[i];
                Traverse ftrv = trv.Field(name);
				Assert.IsNotNull(ftrv);

				Assert.AreEqual(TraverseFields.testStrings[i], ftrv.GetValue());
				Assert.AreEqual(TraverseFields.testStrings[i], ftrv.GetValue<string>());
			}
		}

		// Traverse.SetValue() should set the value of a traversed field
		// regardless of its access modifier
		//
		[TestMethod]
		public void Traverse_Field_SetValue()
		{
            TraverseFields_AccessModifiers instance = new TraverseFields_AccessModifiers(TraverseFields.testStrings);
            Traverse trv = Traverse.Create(instance);

			for (int i = 0; i < TraverseFields.testStrings.Length; i++)
			{
                string newValue = "newvalue" + i;

				// before
				Assert.AreEqual(TraverseFields.testStrings[i], instance.GetTestField(i));

                string name = TraverseFields.fieldNames[i];
                Traverse ftrv = trv.Field(name);
				ftrv.SetValue(newValue);

				// after
				Assert.AreEqual(newValue, instance.GetTestField(i));
				Assert.AreEqual(newValue, ftrv.GetValue());
				Assert.AreEqual(newValue, ftrv.GetValue<string>());
			}
		}
	}
}
