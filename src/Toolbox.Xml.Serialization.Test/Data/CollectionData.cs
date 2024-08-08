using System.Collections.Generic;

namespace Toolbox.Xml.Serialization.Test.Data
{
    class CollectionData
    {
        public List<string> Names { get; set; }
		public List<int> Numbers { get; set; }
		public Dictionary<string, string> Dictionary { get; set; }
        public Stack<string> Stacked { get; set; }
        public Queue<string> Queued { get; set; }

    }
}
