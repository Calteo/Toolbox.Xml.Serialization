using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Xml.Serialization;

namespace Toolbox.Xml.Serialisation.Test.Data
{
    class MethodText
    {
        public string Name { get; set; }

        [NotSerialized]
        public int Hidden { get; set; }

        public string HiddenText { get; set; }

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
            HiddenText = Hidden.ToString();
            OnSerializingCalled = true;
        }

        [OnSerialized]
        private void OnSerialized(Dictionary<string, string> data)
        {
            HiddenText = null;
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
            Hidden = int.Parse(HiddenText);
            HiddenText = null;
            OnDeserializedCalled = true;
        }
    }
}
