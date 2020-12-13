using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Xml.Serialization.Test.Data
{
    class SafeData
    {
        [Obfuscate]
        public string Password { get; set; }

        [OnSerializing]
        private void OnSerializing(Dictionary<string, string> data)
        {
            data["Hidden"] = "Some hidden text.";
        }

    }
}
