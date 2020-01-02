﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Xml.Serialization;

namespace Toolbox.Xml.Serialisation.Test.Data
{
    class SomeData
    {
        public SomeData()
        {
            YesNo = true;
            Value = 42;
            Number = 3.14M;
            SubData = new SubData();
            DataAfterObject = $"Some data after object reference at {GetHashCode()}";
            NotGood = $"This is {GetHashCode()}";
            Products = new List<string>();
        }

        public string Name { get; set; }
        public int Value { get; set; }
        public bool YesNo { get; set; }

        [NotSerialized]
        public string NotGood { get; set; }

        public decimal Number { get; set; }
        public SubData SubData { get; set; }
        public string DataAfterObject { get; set; }
        public string[] Names { get; set; }

        public List<string> Products { get; set; }
    }
}
