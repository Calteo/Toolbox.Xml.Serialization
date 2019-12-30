﻿using System;
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
        public void Serialize(T obj, string fileName, SaveOptions saveOptions = SaveOptions.OmitDuplicateNamespaces)
        {
            var document = Serialize(obj);
            document.Save(fileName, saveOptions);
        }

        private const string ItemName = "Item";
        private const string TypeName = "type";
        private const string SystemNamespacePrefix = "sys";

        private XNamespace SystemNamespace = "http://me";


        private Dictionary<Type, string> ExtendedTypes { get; } = new Dictionary<Type, string>();
        private XAttribute GetExtendedTypeAttribute(Type type)
        {
            if (!ExtendedTypes.TryGetValue(type, out string name))
            {                
                name = $"t{ExtendedTypes.Count + 1}";
                ExtendedTypes[type] = name;
            }
            return new XAttribute(SystemNamespace + TypeName, name);
        }
        
        private XDocument Serialize(T obj)
        {
            ExtendedTypes.Clear();

            var document = new XDocument();

            var root = SerializeObject(typeof(T).Name, obj, typeof(T));
            if (ExtendedTypes.Count > 0)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + SystemNamespacePrefix, SystemNamespace));
                foreach (var kvp in ExtendedTypes.OrderBy(kvp => kvp.Value))
                {
                    root.Add(new XAttribute(SystemNamespace + kvp.Value, kvp.Key.AssemblyQualifiedName));
                }
            }

            document.Add(root);

            return document;
        }

        private XElement SerializeObject(string name, object obj, Type expectedType)
        {
            var type = obj.GetType();
            if (type.GetConstructor(Type.EmptyTypes) == null)
                return null;

            var element = new XElement(expectedType.Name);
            if (expectedType != type)
            {
                element.Add(GetExtendedTypeAttribute(type));
            }

            var properties = GetProperties(type);

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

            return SerializeValue(property.Name, property.GetValue(obj), property.PropertyType);
        }

        private XElement SerializeValue(string name, object value, Type expectedType)
        {
            var type = value?.GetType();

            if (type == null)
                return new XElement(name);

            if (type.IsArray)
            {
                return SerializeArray(name, (Array)value, expectedType);
            }
            else if (type == typeof(string))
            {
                return SerializeString(name, (string)value);
            }
            else if (type.IsValueType)
            {
                return SerializeValueType(name, value);
            }
            else
            {
                return SerializeObject(name, value, expectedType);
            }
        }

        private XElement SerializeArray(string name, Array value, Type expectedType)
        {            
            if (value.Rank != 1) return null;

            var element = new XElement(name);
            

            for (var i = 0; i < value.Length; i++)
            {
                var elementValue = value.GetValue(i);
                var itemElement = SerializeValue(ItemName, elementValue, expectedType.GetElementType());
                if (itemElement != null)
                {
                    element.Add(itemElement);
                }
            }

            return element;
        }

        private XElement SerializeString(string name, string value)
        {
            var element = new XElement(name);

            if (value != null)
                element.Add(new XText(value));

            return element;
        }

        private XElement SerializeValueType(string name, object value)
        {
            var element = new XElement(name);

            if (value != null)
            {
                element.Add(new XText(Convert.ToString(value, CultureInfo.InvariantCulture)));
            }

            return element;
        }

        private Dictionary<string, Type> DeserializeTypes { get; } = new Dictionary<string, Type>();

        /// <summary>
        /// Deserialize an object from a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The deserialized object</returns>
        public T Deserialize(string fileName)
        {
            var document = XDocument.Load(fileName, LoadOptions.SetLineInfo);

            DeserializeTypes.Clear();
            var attributes = document.Root.Attributes().Where(a => a.Name.Namespace == SystemNamespace);

            foreach (var attribute in attributes)
            {
                DeserializeTypes.Add(attribute.Name.LocalName, Type.GetType(attribute.Value, true));
            }

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

            var propertyElement = element.Elements().FirstOrDefault(e => e.Name.LocalName == property.Name);
            if (propertyElement == null) return;

            object value = DeserializeValue(propertyElement, property.PropertyType);

            property.SetValue(obj, value);
        }

        private object DeserializeValue(XElement element, Type type)
        {
            if (element.IsEmpty)
                return null;

            var typeAttribute = element.Attribute(SystemNamespace + TypeName);
            if (typeAttribute != null)
            {
                type = DeserializeTypes[typeAttribute.Value];
            }

            if (type.IsArray)
                return DeserializeArray(element, type);

            if (type == typeof(string))
                return DeserializeString(element);
            
            if (type.IsValueType)
                return DeserializeValueType(element, type);

            return DeserializeObject(element, type);            
        }

        private object DeserializeArray(XElement element, Type type)
        {
            var items = element.Elements().Where(e => e.Name.LocalName == ItemName).ToArray();
            var elementType = type.GetElementType();
            var obj = Array.CreateInstance(type.GetElementType(), items.Length);
            for (var i = 0; i < items.Length; i++)
            {
                var value = DeserializeValue(items[i], elementType);
                obj.SetValue(value, i);
            }
            return obj;
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
