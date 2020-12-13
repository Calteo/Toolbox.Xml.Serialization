using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Xml.Serialisation.Test.Data
{
    class ArrayData
    {
        public ArrayData()
        {
        }

        public string[] Names { get; set; }

        public int[,] Numbers { get; set; }
    }
}
