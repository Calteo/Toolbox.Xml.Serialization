using System;

namespace Toolbox.Xml.Serialization
{
    /// <summary>
    /// Marks a property to be not serialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class XmlNonSerializedAttribute : Attribute
    {
    }
}
