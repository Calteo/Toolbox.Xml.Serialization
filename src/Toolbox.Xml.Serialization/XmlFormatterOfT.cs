using System.IO;

namespace Toolbox.Xml.Serialization
{
    /// <summary>
    /// The generic formatter class.
    /// </summary>
    /// <typeparam name="T">A type with a default constructor.</typeparam>
    public class XmlFormatter<T> : XmlFormatter where T : class, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XmlFormatter{T}"/>.
        /// </summary>
        public XmlFormatter() : base(typeof(T))
        {
        }

        /// <summary>
        /// Deserialize an object of type T from a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The deserialized object</returns>
        public new T Deserialize(string fileName)
        {
            return (T)base.Deserialize(fileName);
        }

        /// <summary>
        /// Deserialize an object of type T from a stream.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The deserialized object</returns>
        public new T Deserialize(Stream stream)
        {
            return (T)base.Deserialize(stream);
        }

    }
}
