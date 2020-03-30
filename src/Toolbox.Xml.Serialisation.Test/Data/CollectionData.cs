using System.Collections.Generic;

namespace Toolbox.Xml.Serialisation.Test.Data
{
    class CollectionData
    {
        public List<string> Names { get; set; }
        public Dictionary<string, string> Dictionary { get; set; }
        public Stack<string> Stacked { get; set; }
        public Queue<string> Queued { get; set; }

    }
}
