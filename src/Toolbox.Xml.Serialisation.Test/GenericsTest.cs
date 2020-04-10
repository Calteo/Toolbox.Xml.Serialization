using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml.Serialisation.Test.Data;
using Toolbox.Xml.Serialization;

namespace Toolbox.Xml.Serialisation.Test
{
    [TestClass]
    public class GenericsTest
    {
        public GenericsTest()
        {
            Filename = "generics.xml";
        }

        public string Filename { get; }


        [TestMethod]
        public void ContainerTest()
        {
            var cut = new XmlFormatter<Container<string>>();

            var input = new Container<string> { Data = "Hello" };

            cut.Serialize(input, Filename);
            var read = cut.Deserialize(Filename);

            Assert.AreEqual(input.Data, read.Data);
        }

        [TestMethod]
        public void MemroyTest()
        {
            var cut = new XmlFormatter<Container<string>>();

            var input = new Container<string> { Data = "Hello" };

            var stream = new MemoryStream();

            cut.Serialize(input, stream);
            stream.Position = 0;

            var read = cut.Deserialize(stream);

            Assert.AreEqual(input.Data, read.Data);
        }
    }
}
