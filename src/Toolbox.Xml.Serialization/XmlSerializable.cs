using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Xml.Serialization
{
    /// <summary>
    /// Marks an object for usage by the <see cref="XmlFormatter{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    sealed public class XmlSerializeableAttribute : Attribute
    {
        public XmlSerializeableAttribute(string name = null)
        {
            Name = name;
        }

        public string Name { get; private set; } 
    }
}
