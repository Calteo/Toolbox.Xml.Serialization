using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml.Serialisation.Test.Data;
using Toolbox.Xml.Serialization;

namespace Toolbox.Xml.Serialisation.Test
{
    /// <summary>
    /// Summary description for ArrayTest
    /// </summary>
    [TestClass]
    public class ArrayTest
    {
        public ArrayTest()
        {
            Filename = "arraytest.xml";
        }

        public string Filename { get; }

        [TestMethod]
        public void SimpleArrayTest()
        {
            var cut = new XmlFormatter<ArrayData>();

            var data = new ArrayData
            {
                Names = new[] { "Alice", "Bob", "Charles" }
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Names.Length, read.Names.Length);
            for (int i = 0; i < data.Names.Length; i++)
            {
                Assert.AreEqual(data.Names[i], read.Names[i], $"Names[{i}]");
            }
        }

        [TestMethod]
        public void MultiArrayTest()
        {
            var cut = new XmlFormatter<ArrayData>();

            var data = new ArrayData
            {
                Numbers = new int[3,2]
                {
                    { 1,2 }, { 3,4 }, { 5,6 }
                }
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            var dataN = data.Numbers;
            var readN = read.Numbers;
            
            Assert.AreEqual(dataN.Length, readN.Length);
            Assert.AreEqual(dataN.Rank, readN.Rank);
            Assert.AreEqual(dataN.GetUpperBound(0), readN.GetUpperBound(0));
            Assert.AreEqual(dataN.GetUpperBound(1), readN.GetUpperBound(1));

            for (var i = 0; i < dataN.GetUpperBound(0); i++)
            {
                for (int j = 0; j < dataN.GetUpperBound(1); j++)
                {
                    Assert.AreEqual(dataN[i, j], readN[i, j], $"Array[{i},{j}]");
                }
            }
        }
    }
}
