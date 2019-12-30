using System;

namespace Toolbox.Xml.Serialization
{
    /// <summary>
    /// Marks a property to be not serialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class NotSerializedAttribute : Attribute
    {
    }
}