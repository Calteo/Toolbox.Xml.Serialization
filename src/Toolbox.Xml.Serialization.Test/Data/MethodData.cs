using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Xml.Serialization;

namespace Toolbox.Xml.Serialization.Test.Data
{
    class MethodData
    {
        public string Name { get; set; }
        public string Data { get; set; } = "To interfere with the additional data.";

        [NotSerialized]
        public int Hidden { get; set; }

        [NotSerialized]
        public bool OnSerializingCalled { get; set; }

        [NotSerialized]
        public bool OnSerializedCalled { get; set; }

        [NotSerialized]
        public bool OnDeserializingCalled { get; set; }

        [NotSerialized]
        public bool OnDeserializedCalled { get; set; }

        [OnSerializing]
        private void OnSerializing(Dictionary<string, string> data)
        {
            data["Hidden"] = Hidden.ToString();
            OnSerializingCalled = true;
        }

        [OnSerialized]
        private void OnSerialized(Dictionary<string, string> data)
        {
            OnSerializedCalled = true;
        }

        [OnDeserializing]
        private void OnDeserializing(Dictionary<string, string> data)
        {
            OnDeserializingCalled = true;
        }

        [OnDeserialized]
        private void OnDeserialized(Dictionary<string, string> data)
        {
            Hidden = int.Parse(data["Hidden"]);
            OnDeserializedCalled = true;
        }
    }
}
