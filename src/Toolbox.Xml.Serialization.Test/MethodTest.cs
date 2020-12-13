using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Xml.Serialization;
using Toolbox.Xml.Serialization.Test.Data;

namespace Toolbox.Xml.Serialization.Test
{
    [TestClass]
    public class MethodTest
    {
        public MethodTest()
        {
            Filename = "methods.xml";
        }

        public string Filename { get; }

        [TestMethod]
        public void MethodToHiddenTextTest()
        {
            var cut = new XmlFormatter<MethodText>();

            var input = new MethodText { Name = "John", Hidden = 42 };

            cut.Serialize(input, Filename);
            
            var read = cut.Deserialize(Filename);

            Assert.IsTrue(input.OnSerializingCalled);
            Assert.IsTrue(input.OnSerializedCalled);
            Assert.IsNull(input.HiddenText);
            Assert.IsFalse(read.OnSerializingCalled);
            Assert.IsFalse(read.OnSerializedCalled);

            Assert.IsTrue(read.OnDeserializingCalled);
            Assert.IsTrue(read.OnDeserializedCalled);
            Assert.IsNull(read.HiddenText);
            Assert.AreEqual(input.Name, read.Name);
            Assert.AreEqual(input.Hidden, read.Hidden);
            
        }

        [TestMethod]
        public void MethodToHiddenDataTest()
        {
            var cut = new XmlFormatter<MethodData>();

            var input = new MethodData { Name = "John", Hidden = 42 };

            cut.Serialize(input, Filename);

            var read = cut.Deserialize(Filename);

            Assert.IsTrue(input.OnSerializingCalled);
            Assert.IsTrue(input.OnSerializedCalled);
            Assert.IsFalse(read.OnSerializingCalled);
            Assert.IsFalse(read.OnSerializedCalled);

            Assert.IsTrue(read.OnDeserializingCalled);
            Assert.IsTrue(read.OnDeserializedCalled);
            Assert.AreEqual(input.Name, read.Name);
            Assert.AreEqual(input.Data, read.Data);
            Assert.AreEqual(input.Hidden, read.Hidden);
        }
    }
}
