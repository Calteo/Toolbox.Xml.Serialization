using System;

namespace Toolbox.Xml.Serialization
{
    /// <summary>
    /// Obfuscates a property in the serialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ObfuscateAttribute : Attribute
    {
    }
}
