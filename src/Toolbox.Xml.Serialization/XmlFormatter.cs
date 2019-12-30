using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Toolbox.Xml.Serialization
{
    /// <summary>
    /// The basic formatter class.
    /// </summary>
    /// <typeparam name="T">A class with a default constructor.</typeparam>
    public class XmlFormatter<T> where T : class, new()
    {
        /// <summary>
        /// Serializes an object to a file.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="fileName">a filename</param>
        /// <param name="saveOptions">options for saving</param>
        public void Serialize(T obj, string fileName, SaveOptions saveOptions = SaveOptions.None)
        {
            var document = Serialize(obj);
            document.Save(fileName, saveOptions);
        }
        
        private XDocument Serialize(T obj)
        {
            var document = new XDocument();
            
            document.Add(SerializeObject(obj));

            return document;
        }

        private XElement SerializeObject(object obj)
        {
            if (obj.GetType().GetConstructor(Type.EmptyTypes) == null)
                return null;

            var element = new XElement(obj.GetType().Name);

            var properties = GetProperties(obj.GetType());

            foreach (var property in properties)
            {
                var propertyElement = Serialize(obj, property);
                if (propertyElement != null)
                    element.Add(propertyElement);            
            }

            return element;
        }
        
        private XElement Serialize(object obj, PropertyInfo property)
        {
            if (!property.CanRead) return null;

            if (property.PropertyType.IsArray)
            {
                // TODO: array
                return null;
            }
            else if (property.PropertyType == typeof(string))
            {
                return SerializeString(obj, property);
            }
            else if (property.PropertyType.IsValueType)
            {
                return SerializeValueType(obj, property);
            }
            else
            {
                var value = property.GetValue(obj);
                if (value == null)
                    return new XElement(property.Name);

                return SerializeObject(value);
            }
        }

        private XElement SerializeString(object obj, PropertyInfo property)
        {
            var element = new XElement(property.Name);

            var value = (string)property.GetValue(obj);
            if (value != null)
                element.Add(new XText(value));

            return element;
        }

        private XElement SerializeValueType(object obj, PropertyInfo property)
        {
            var element = new XElement(property.Name);

            var value = property.GetValue(obj);
            if (value != null)
            {
                element.Add(new XText(Convert.ToString(value, CultureInfo.InvariantCulture)));
            }

            return element;
        }

        /// <summary>
        /// Deserialize an object from a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The deserialized object</returns>
        public T Deserialize(string fileName)
        {
            var document = XDocument.Load(fileName, LoadOptions.SetLineInfo);
            return (T)DeserializeObject(document.Root, typeof(T));
        }

       
        private object DeserializeObject(XElement element, Type type) 
        {
            var obj = Activator.CreateInstance(type);
            var properties = GetProperties(type);

            foreach (var property in properties)
            {
                DeserializeProperty(element, obj, property);
            }

            return obj;
        }

        private void DeserializeProperty(XElement element, object obj, PropertyInfo property)
        {
            if (!property.CanWrite) return;

            var propertyElement = element.Element(property.Name);
            if (propertyElement == null) return;

            object value = null;

            if (propertyElement.IsEmpty)
            {                
            }            
            else if (property.PropertyType.IsArray)
            {
                // TODO: array
            }
            else if (property.PropertyType == typeof(string))
            {
                value = DeserializeString(propertyElement);
            }
            else if (property.PropertyType.IsValueType)
            {
                value = DeserializeValueType(propertyElement, property.PropertyType);
            }
            else
            {
                value = DeserializeObject(propertyElement, property.PropertyType);
            }

            property.SetValue(obj, value);
        }

        private string DeserializeString(XElement element)
        {
            return (element.FirstNode as XText)?.Value;            
        }

        private object DeserializeValueType(XElement element, Type type)
        {
            var text = (element.FirstNode as XText)?.Value;
            return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<NotSerializedAttribute>() == null);
        }
    }
}
