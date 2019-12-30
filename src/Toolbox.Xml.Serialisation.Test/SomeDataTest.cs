using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml.Serialisation.Test.Data;
using Toolbox.Xml.Serialization;

namespace Toolbox.Xml.Serialisation.Test
{
    [TestClass]
    public class SomeDataTest
    {
        public SomeDataTest()
        {
            Filename = "somedata.xml";
        }

        public string Filename { get; }

        [TestMethod]
        public void WriteAndReadSomeData()
        {
            var cut = new XmlFormatter<SomeData>();
            var data = new SomeData
            {
                Name = null,
                Number = 0.42M + GetHashCode(),
                DataAfterObject = $"Some text after object at {GetHashCode()}",
                Value = GetHashCode(),
                SubData = new SubData
                {
                    Info = $"Info at {GetHashCode()}"
                },
                Names = new[] {"Name1", "Name2"},
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Name, read.Name);
            Assert.AreEqual(data.Number, read.Number);
            Assert.AreEqual(data.YesNo, read.YesNo);
            Assert.AreEqual(data.Value, read.Value);
            Assert.AreEqual(data.SubData.Info, read.SubData.Info);
            Assert.AreEqual(data.DataAfterObject, read.DataAfterObject);
            Assert.AreNotEqual(data.NotGood, read.NotGood);
            Assert.AreEqual(data.Names.Length, read.Names.Length);
            for (var i = 0; i < data.Names.Length; i++)
            {
                Assert.AreEqual(data.Names[i], read.Names[i], $"Names[{i}] differ");
            }
        }

        [TestMethod]
        public void WriteAndReadSomeDataWithCultureName()
        {
            var cut = new XmlFormatter<SomeData>();
            var data = new SomeData
            {
                Name = "äöü ÄÖÜ ß &<>"
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Name, read.Name);
        }

        [TestMethod]
        public void WriteAndReadSomeDataWithBadSubData()
        {
            var cut = new XmlFormatter<SomeData>();
            var data = new SomeData
            {
                Name = null,
                Number = 0.42M + GetHashCode(),
                DataAfterObject = $"Some text after object at {GetHashCode()}",
                Value = GetHashCode(),
                SubData = new BadSubData($"Info at {GetHashCode()}"),
                Names = new[] { "Name1", "Name2" },
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Name, read.Name);
            Assert.AreEqual(data.Number, read.Number);
            Assert.AreEqual(data.YesNo, read.YesNo);
            Assert.AreEqual(data.Value, read.Value);
            Assert.AreNotEqual(data.SubData.Info, read.SubData.Info);
            Assert.AreEqual(data.DataAfterObject, read.DataAfterObject);
            Assert.AreNotEqual(data.NotGood, read.NotGood);
            Assert.AreEqual(data.Names.Length, read.Names.Length);
            for (var i = 0; i < data.Names.Length; i++)
            {
                Assert.AreEqual(data.Names[i], read.Names[i], $"Names[{i}] differ");
            }
        }
    }
}
