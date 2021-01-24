using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Xml.Serialization;

namespace Toolbox.Xml.Serialization.Test.Data
{
    class SomeData
    {
        public SomeData()
        {
            YesNo = true;
            Value = GetHashCode();
            Number = 3.14M;
            SubData = new SubData();
            DataAfterObject = $"Some data after object reference at {GetHashCode()}";
            NotGood = $"This is {GetHashCode()}";
            Products = new List<string>();
            SubDatas = new Dictionary<string, SubData>();
        }

        public string Name { get; set; }
        public int Value { get; private set; }
        public bool YesNo { get; set; }

        public DateTime Time { get; set; }
        public TimeSpan Span { get; set; }

        [NotSerialized]
        public string NotGood { get; set; }

        public decimal Number { get; set; }
        public SubData SubData { get; set; }
        public string DataAfterObject { get; set; }
        public string[] Names { get; set; }

        public List<string> Products { get; set; }

        public Dictionary<string, SubData> SubDatas { get; set; }

        public Point Location { get; set; }
    }
}
