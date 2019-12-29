using System;

namespace Toolbox.Xml.Serialization
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class XmlNotSerializedAttribute : Attribute
    {
    }
}
