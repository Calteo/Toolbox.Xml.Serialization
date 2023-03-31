using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml.Serialization.Test.Data;

namespace Toolbox.Xml.Serialization.Test
{
    [TestClass]
    public class ObfuscatedTest
    {
        [TestMethod]
        public void SaveAndLoad()
        {
            const string Name = "SomeName";
            const string Secret = "SuperSecret";

            var data = new SecureData
            {
                Name = Name,
                Secret = Secret
            };

            var stream = new MemoryStream();
            var formatter = new XmlFormatter<SecureData>();
            formatter.Serialize(data, stream);

            stream.Position = 0;

            var cut = formatter.Deserialize(stream);

            Assert.IsNotNull(cut);
            Assert.AreEqual(Name, cut.Name);
            Assert.AreEqual(Secret, cut.Secret);
        }
    }
}
