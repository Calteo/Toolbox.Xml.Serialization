using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml.Serialization;
using Toolbox.Xml.Serialization.Test.Data;

namespace Toolbox.Xml.Serialization.Test
{
	[TestClass]
	public class NumberTest
	{
		public NumberTest()
		{
			Filename = "numbers.xml";
		}

		public string Filename { get; }

		[TestMethod]
		public void NumberULongTest()
		{
			var cut = new XmlFormatter<Number<ulong>>();

			var input = new Number<ulong>();
			input.Values[0] = 1;

			cut.Serialize(input, Filename);
			var read = cut.Deserialize(Filename);

			Assert.AreEqual(input.Zero, read.Zero);
			Assert.AreEqual(input.One, read.One);
			Assert.AreEqual(input.Values.Count, read.Values.Count);
			Assert.AreEqual(input.Values[0], read.Values[0]);
		}
	}
}
