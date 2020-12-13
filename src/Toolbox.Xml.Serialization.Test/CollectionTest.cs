using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml.Serialization;
using Toolbox.Xml.Serialization.Test.Data;

namespace Toolbox.Xml.Serialization.Test
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


        [TestMethod]
        public void QueueTest()
        {
            var cut = new XmlFormatter<CollectionData>();

            var data = new CollectionData
            {
                Queued = new Queue<string>(new[] { "Alice", "Bob", "Charles" })
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Queued.Count, read.Queued.Count);
            while (read.Queued.Count > 0)
                Assert.AreEqual(data.Queued.Dequeue(), read.Queued.Dequeue());
        }

        [TestMethod]
        public void DictionaryTest()
        {
            var cut = new XmlFormatter<CollectionData>();

            var data = new CollectionData
            {
                Dictionary = new Dictionary<string, string>
                {
                    { "One", "Alice" },
                    { "Two", "Bob" },
                    { "Three", "Chales" },
                }
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Dictionary.Count, read.Dictionary.Count);
            foreach (var key in data.Dictionary.Keys)
                Assert.AreEqual(data.Dictionary[key], read.Dictionary[key], $"Dicitionary[{key}]");
        }


        [TestMethod]
        public void ListOfTTest()
        {
            var cut = new XmlFormatter<CollectionData>();

            var data = new CollectionData
            {
                Names = new List<string>{ "Alice", "Bob", "Charles" }
            };

            cut.Serialize(data, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(data.Names.Count, read.Names.Count);
            for (var i = 0; i < read.Names.Count; i++)
                Assert.AreEqual(data.Names[i], read.Names[i], $"Names[{i}]");
        }
    }
}
