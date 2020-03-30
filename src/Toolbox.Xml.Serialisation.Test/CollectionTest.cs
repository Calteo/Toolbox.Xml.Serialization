using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml.Serialisation.Test.Data;
using Toolbox.Xml.Serialization;

namespace Toolbox.Xml.Serialisation.Test
{
    [TestClass]
    public class CollectionTest
    {
        public CollectionTest()
        {
            Filename = "collection.xml";
        }

        public string Filename { get; }

        [TestMethod]
        public void StackTest()
        {
            var cut = new XmlFormatter<CollectionData>();

            var data = new CollectionData
            {
                Stacked = new Stack<string>(new[] { "Alice", "Bob", "Charles" })
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Stacked.Count, read.Stacked.Count);
            while (read.Stacked.Count>0)
                Assert.AreEqual(data.Stacked.Pop(), read.Stacked.Pop());
        }
    }
}
