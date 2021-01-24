using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.IO;
using System.Security;
using Toolbox.Xml.Serialization;
using Toolbox.Xml.Serialization.Test.Data;

namespace Toolbox.Xml.Serialization.Test
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
        public void WriteAndRead()
        {
            var cut = new XmlFormatter<SomeData>();
            var data = new SomeData
            {
                Name = null,
                Number = 0.42M + GetHashCode(),
                Time = DateTime.Now,
                Span = TimeSpan.FromMilliseconds(457575578),
                DataAfterObject = $"Some text after object at {GetHashCode()}",                
                SubData = new SubData
                {
                    Info = $"Info at {GetHashCode()}"
                },
                Names = new[] {"Name1", "Name2"},
                Products = { "product1", "product2", "product3" },
                SubDatas =
                {
                    { "Test1", new SubData { Info = $"Test1 dictionary at {GetHashCode()}" } },
                    { "Test2", new SubData { Info = $"Test2 dictionary at {GetHashCode()}" } }
                },
                Location = new Point(5,6)
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Name, read.Name);
            Assert.AreEqual(data.Number, read.Number);
            Assert.AreEqual(data.Time, read.Time);
            Assert.AreEqual(data.Span, read.Span);
            Assert.AreEqual(data.YesNo, read.YesNo);
            Assert.AreEqual(data.Value, read.Value);
            Assert.AreEqual(data.Location, read.Location);
            Assert.AreEqual(data.SubData.Info, read.SubData.Info);
            Assert.AreEqual(data.DataAfterObject, read.DataAfterObject);
            Assert.AreNotEqual(data.NotGood, read.NotGood);
            Assert.AreEqual(data.Names.Length, read.Names.Length);
            for (var i = 0; i < data.Names.Length; i++)
            {
                Assert.AreEqual(data.Names[i], read.Names[i], $"Names[{i}] differ");
            }
            Assert.AreEqual(data.Products.Count, read.Products.Count);
            for (var i = 0; i < data.Products.Count; i++)
            {
                Assert.AreEqual(data.Products[i], read.Products[i], $"Products[{i}] differ");
            }
            Assert.AreEqual(data.SubDatas.Count, read.SubDatas.Count);
            foreach (var key in data.SubDatas.Keys)
            {
                Assert.AreEqual(data.SubDatas[key].Info, read.SubDatas[key].Info);
            }
        }

        [TestMethod]
        public void WriteAndReadWithoutGenerics()
        {
            var cut = new XmlFormatter(typeof(SomeData));
            var data = new SomeData
            {
                Name = null,
                Number = 0.42M + GetHashCode(),
                DataAfterObject = $"Some text after object at {GetHashCode()}",
                SubData = new SubData
                {
                    Info = $"Info at {GetHashCode()}"
                },
                Names = new[] { "Name1", "Name2" },
                Products = { "product1", "product2", "product3" },
                SubDatas =
                {
                    { "Test1", new SubData { Info = $"Test1 dictionary at {GetHashCode()}" } },
                    { "Test2", new SubData { Info = $"Test2 dictionary at {GetHashCode()}" } }
                }
            };

            cut.Serialize(data, Filename);
            var read = (SomeData)cut.Deserialize(Filename);

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
            Assert.AreEqual(data.Products.Count, read.Products.Count);
            for (var i = 0; i < data.Products.Count; i++)
            {
                Assert.AreEqual(data.Products[i], read.Products[i], $"Products[{i}] differ");
            }
            Assert.AreEqual(data.SubDatas.Count, read.SubDatas.Count);
            foreach (var key in data.SubDatas.Keys)
            {
                Assert.AreEqual(data.SubDatas[key].Info, read.SubDatas[key].Info);
            }
        }

        [TestMethod]
        public void WriteAndReadCultureName()
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
        public void WriteAndReadBadSubData()
        {
            var cut = new XmlFormatter<SomeData>();
            var data = new SomeData
            {
                Name = null,
                Number = 0.42M + GetHashCode(),
                DataAfterObject = $"Some text after object at {GetHashCode()}",
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
        [TestMethod]
        public void WriteAndReadDerivedSubData()
        {
            var cut = new XmlFormatter<SomeData>();
            var data = new SomeData
            {
                Name = null,
                Number = 0.42M + GetHashCode(),
                DataAfterObject = $"Some text after object at {GetHashCode()}",
                SubData = new DerivedSubData
                {
                    Info = $"Info at {GetHashCode()}",
                    MoreInfo = $"MoreInfo at {GetHashCode()}"
                },
                Names = new[] { "Name1", "Name2" },
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Name, read.Name);
            Assert.AreEqual(data.Number, read.Number);
            Assert.AreEqual(data.YesNo, read.YesNo);
            Assert.AreEqual(data.Value, read.Value);
            Assert.AreEqual(data.SubData.Info, read.SubData.Info);
            Assert.IsInstanceOfType(read.SubData, data.SubData.GetType());
            var subData = (DerivedSubData)data.SubData;
            var readSubData = (DerivedSubData)read.SubData;
            Assert.AreEqual(subData.MoreInfo, readSubData.MoreInfo);
            Assert.AreEqual(data.DataAfterObject, read.DataAfterObject);
            Assert.AreNotEqual(data.NotGood, read.NotGood);
            Assert.AreEqual(data.Names.Length, read.Names.Length);
            for (var i = 0; i < data.Names.Length; i++)
            {
                Assert.AreEqual(data.Names[i], read.Names[i], $"Names[{i}] differ");
            }
        }

        [TestMethod]
        public void ReadOldDerivedSubData()
        {
            var cut = new XmlFormatter<SomeData>();

            var read = cut.Deserialize(@"Data\SomeDataWithDerivedSubData.xml");

            Assert.IsNotNull(read);
            Assert.IsInstanceOfType(read.SubData, typeof(DerivedSubData));
            var readSubData = (DerivedSubData)read.SubData;
            Assert.AreEqual(readSubData.MoreInfo, "MoreInfo at 64019460");
        }

        [TestMethod]
        public void WriteAndReadSafeData()
        {
            var cut = new XmlFormatter<SafeData>();

            const string Secret = "S*ecret";

            var data = new SafeData
            {
                Password = Secret
            };

            var stream = new MemoryStream();

            cut.Serialize(data, stream);

            stream.Position = 0;

            var read = cut.Deserialize(stream);

            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                Assert.IsTrue(!text.Contains(Secret), "serialization contains secret");
            }

            Assert.AreEqual(data.Password, read.Password);
        }

        [TestMethod]
        public void WriteAndReadSafeDataDifferentFormatters()
        {
            const string EncryptionKey = "SuperSecretKey";

            var cut = new XmlFormatter<SafeData>(EncryptionKey);

            const string Secret = "S*ecret";

            var data = new SafeData
            {
                Password = Secret
            };

            var stream = new MemoryStream();

            new XmlFormatter<SafeData>(EncryptionKey).Serialize(data, stream);

            stream.Position = 0;

            var read = cut.Deserialize(stream);

            Assert.AreEqual(data.Password, read.Password);
        }
    }
}
