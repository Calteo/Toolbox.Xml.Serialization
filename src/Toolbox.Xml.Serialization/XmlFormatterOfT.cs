namespace Toolbox.Xml.Serialization
{
    /// <summary>
    /// The basic formatter class.
    /// </summary>
    /// <typeparam name="T">A class with a default constructor.</typeparam>
    public class XmlFormatter<T> : XmlFormatter where T : class, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XmlFormatter{T}"/>.
        /// </summary>
        public XmlFormatter() : base(typeof(T))
        {
        }

        public new T Deserialize(string fileName)
        {
            return (T)base.Deserialize(fileName);
        }
    }
}
