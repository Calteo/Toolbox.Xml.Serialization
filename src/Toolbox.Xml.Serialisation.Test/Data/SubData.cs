using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Xml.Serialization;

namespace Toolbox.Xml.Serialisation.Test.Data
{
    [XmlSerializeable]
    class SubData
    {
        public SubData()
        {
            Info = $"Created on {DateTime.Now}";
        }

        public string Info { get; set; }
    }
}
