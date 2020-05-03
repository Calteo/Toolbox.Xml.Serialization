﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Toolbox.Xml.Serialization
{
    /// <summary>
    /// The basic formatter class
    /// </summary>
    public class XmlFormatter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XmlFormatter"/> instance.
        /// </summary>
        /// <param name="type">The root type. It must implement a default constructor.</param>
        public XmlFormatter(Type type)
        {
            if (!type.IsClass)
                throw new ArgumentException("only classes can be serialized", nameof(type));
            if (type.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException("only classes with default constructor can be serialized", nameof(type));

            RootType = type;
        }

        private Type RootType { get; }

        private Dictionary<Type, List<MethodInfo>> SerializingMethods { get; } = new Dictionary<Type, List<MethodInfo>>();
        private Dictionary<Type, List<MethodInfo>> SerializedMethods { get; } = new Dictionary<Type, List<MethodInfo>>();
        private Dictionary<Type, List<MethodInfo>> DeserializingMethods { get; } = new Dictionary<Type, List<MethodInfo>>();
        private Dictionary<Type, List<MethodInfo>> DeserializedMethods { get; } = new Dictionary<Type, List<MethodInfo>>();

        private List<MethodInfo> GetHandlers<T>(Type type) where T : Attribute
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetCustomAttribute<T>() != null
                    && m.GetParameters().Length==1 
                    && m.GetParameters()[0].ParameterType == typeof(Dictionary<string,string>))
                    .ToList();
        }

        /// <summary>
        /// Serializes an object to a file.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="fileName">a filename</param>
        /// <param name="saveOptions">options for saving</param>
        public void Serialize(object obj, string fileName, SaveOptions saveOptions = SaveOptions.OmitDuplicateNamespaces)
        {
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Serialize(obj, stream, saveOptions);
            }
        }

        /// <summary>
        /// Serializes an <see cref="object"/> to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="stream">stream to write to</param>
        /// <param name="saveOptions">options for saving</param>
        public void Serialize(object obj, Stream stream, SaveOptions saveOptions = SaveOptions.OmitDuplicateNamespaces)
        {
            var document = Serialize(obj);
            document.Save(stream, saveOptions);
        }

        private const string DataName = "Data";
        private const string KeyName = "Key";
        private const string ValueName = "Value";
        private const string ItemName = "Item";
        private const string ItemsName = "Items";
        private const string TypeName = "type";
        private const string SystemNamespacePrefix = "sys";

        private readonly XNamespace SystemNamespace = "https://github.com/Calteo/Toolbox.Xml.Serialization";

        private Dictionary<Type, string> ExtendedTypes { get; } = new Dictionary<Type, string>();
        private bool NeedSystemNamespace { get; set; }

        private XAttribute GetExtendedTypeAttribute(Type type)
        {
            if (!ExtendedTypes.TryGetValue(type, out string name))
            {
                name = $"t{ExtendedTypes.Count + 1}";
                ExtendedTypes[type] = name;
                NeedSystemNamespace = true;
            }
            return new XAttribute(SystemNamespace + TypeName, name);
        }

        private XDocument Serialize(object obj)
        {
            ExtendedTypes.Clear();

            var type = obj.GetType();

            if (type != RootType)
                throw new ArgumentException($"object is not of type {RootType.FullName}", nameof(obj));

            var document = new XDocument();

            var root = SerializeObject(type.Name, obj, type);
            if (NeedSystemNamespace)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + SystemNamespacePrefix, SystemNamespace));
            }
            if (ExtendedTypes.Count > 0)
            {                
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

            if (!SerializingMethods.TryGetValue(type, out var serializingMethods))
            {
                SerializingMethods[type] = serializingMethods = GetHandlers<OnSerializingAttribute>(type);
            }

            var additionalData = new Dictionary<string, string>();
            serializingMethods.ForEach(p => p.Invoke(obj, new object[] { additionalData }));

            if (type.IsGenericType)
                name = name.Replace('`', '-');

            var element = new XElement(name);
            if (expectedType != type)
            {
                element.Add(GetExtendedTypeAttribute(type));
            }

            if (additionalData.Count > 0)
            {
                NeedSystemNamespace = true;
                var dataElement = new XElement(SystemNamespace + DataName);
                foreach (var kvp in additionalData)
                {
                    dataElement.Add(new XElement(ItemName, new XAttribute(KeyName, kvp.Key), new XAttribute(ValueName, kvp.Value)));
                }

                element.Add(dataElement);

            }

            var properties = GetProperties(type);

            foreach (var property in properties)
            {
                var propertyElement = Serialize(obj, property);
                if (propertyElement != null)
                    element.Add(propertyElement);
            }

            if (!SerializedMethods.TryGetValue(type, out var serializedMethods))
            {
                SerializedMethods[type] = serializedMethods = GetHandlers<OnSerializedAttribute>(type);
            }

            serializedMethods.ForEach(p => p.Invoke(obj, new object[] { additionalData }));

            return element;
        }

        private XElement Serialize(object obj, PropertyInfo property)
        {
            if (!property.CanRead || !property.CanWrite || property.GetIndexParameters().Length != 0) return null;

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
            if (type == typeof(DateTime))
            {
                return SerializeDateTime(name, (DateTime)value);
            }
            if (type == typeof(TimeSpan))
            {
                return SerializeTimeSpan(name, (TimeSpan)value);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var element = new XElement(name);
                var types = type.GetGenericArguments();
                var keyValue = type.GetProperty("Key").GetValue(value);
                var valueValue = type.GetProperty("Value").GetValue(value);

                var keyElement = SerializeValue("Key", keyValue, types[0]);
                var valueElement = SerializeValue("Value", valueValue, types[1]);

                if (keyElement == null || valueElement == null) return null;

                element.Add(keyElement, valueElement);

                return element;
            }
            else if (type.IsValueType)
            {
                return SerializeValueType(name, value);
            }
            else
            {
                var element = SerializeObject(name, value, expectedType);
                if (element != null)
                {
                    Type interfaceType;
                    if ((interfaceType = GetGenericInterface(type, typeof(ICollection<>))) != null)
                        SerializeICollection(interfaceType, value, element);
                    else if (IsGenericType(type, typeof(Stack<>)))
                    {
                        SerializeStack(type, value, element);
                    }
                    else if (IsGenericType(type, typeof(Queue<>)))
                    {
                        SerializeQueue(type, value, element);
                    }
                }
                return element;
            }
        }

        private static Type GetGenericInterface(Type type, Type interfaceType)
        {
            return type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }

        private static bool IsGenericType(Type type, Type genericType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericType;
        }

        private void SerializeICollection(Type interfaceType, object value, XElement element)
        {
            var expectedItemType = interfaceType.GetGenericArguments()[0];
            dynamic collection = value;
            var collectionElement = new XElement(ItemsName);
            element.Add(collectionElement);
            foreach (var item in collection)
            {
                var itemElement = SerializeValue(ItemName, (object)item, expectedItemType);
                if (itemElement != null)
                    collectionElement.Add(itemElement);
            }
        }

        private void SerializeStack(Type type, object value, XElement element)
        {
            var interfaceType = GetGenericInterface(type, typeof(IEnumerable<>));
            SerializeICollection(interfaceType, value, element);
        }

        private void SerializeQueue(Type type, object value, XElement element)
        {
            var interfaceType = GetGenericInterface(type, typeof(IEnumerable<>));
            SerializeICollection(interfaceType, value, element);
        }

        private const string DimensionAttribute = "Dimension";

        private XElement SerializeArray(string name, Array value, Type expectedType)
        {
            var element = new XElement(name);

            var lengths = new List<string>();
            for (var i = 0; i < value.Rank; i++)
            {
                lengths.Add(value.GetLength(i).ToString());
            }

            element.Add(new XAttribute(DimensionAttribute, string.Join(",", lengths)));

            var index = new int[value.Rank];
            var upperBound = new int[value.Rank];

            for (var i = 0; i < value.Rank; i++)
            {
                upperBound[i] = value.GetUpperBound(i);
            }

            var dimension = value.Rank-1;

            while (dimension >= 0)
            {
                var elementValue = value.GetValue(index);

                var itemElement = SerializeValue(ItemName, elementValue, expectedType.GetElementType());
                if (itemElement != null)
                {
                    element.Add(itemElement);
                }

                while (dimension>=0 && ++index[dimension] > upperBound[dimension])
                {
                    index[dimension] = 0;
                    dimension--;
                }
                if (dimension >= 0)
                    dimension = value.Rank - 1;
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

        private XElement SerializeDateTime(string name, DateTime value)
        {
            var element = new XElement(name);

            if (value != null)
                element.Add(new XText(value.ToString("O")));

            return element;
        }
        private XElement SerializeTimeSpan(string name, TimeSpan value)
        {
            var element = new XElement(name);

            if (value != null)
                element.Add(new XText(value.ToString("c")));

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
        public object Deserialize(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Deserialize(stream);
            }
        }

        /// <summary>
        /// Deserialize an object from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>The deserilized object</returns>
        public object Deserialize(Stream stream)
        {
            var document = XDocument.Load(stream, LoadOptions.SetLineInfo);

            DeserializeTypes.Clear();
            var attributes = document.Root.Attributes().Where(a => a.Name.Namespace == SystemNamespace);

            foreach (var attribute in attributes)
            {
                DeserializeTypes.Add(attribute.Name.LocalName, Type.GetType(attribute.Value, true));
            }

            return DeserializeObject(document.Root, RootType);
        }

        private object DeserializeObject(XElement element, Type type)
        {
            var obj = Activator.CreateInstance(type);

            if (!DeserializingMethods.TryGetValue(type, out var deserializingMethods))
            {
                DeserializingMethods[type] = deserializingMethods = GetHandlers<OnDeserializingAttribute>(type);
            }

            var additionalData = new Dictionary<string, string>();

            var dataElement = element.Element(SystemNamespace + DataName);
            if (dataElement != null)
            {
                foreach (var item in dataElement.Elements())
                {
                    additionalData.Add(item.Attribute(KeyName).Value, item.Attribute(ValueName).Value);
                }
            }
                        
            deserializingMethods.ForEach(p => p.Invoke(obj, new object[] { additionalData }));

            var properties = GetProperties(type);

            foreach (var property in properties)
            {
                DeserializeProperty(element, obj, property);
            }

            if (!DeserializedMethods.TryGetValue(type, out var deserializedMethods))
            {
                DeserializedMethods[type] = deserializedMethods = GetHandlers<OnDeserializedAttribute>(type);
            }

            deserializedMethods.ForEach(p => p.Invoke(obj, new object[] { additionalData }));

            return obj;
        }

        private void DeserializeProperty(XElement element, object obj, PropertyInfo property)
        {
            if (!property.CanWrite) return;

            var propertyElement = element.Elements().FirstOrDefault(e => e.Name.NamespaceName=="" && e.Name.LocalName==property.Name);
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

            if (type == typeof(DateTime))
                return DeserializeDateTime(element);

            if (type == typeof(TimeSpan))
                return DeserializeTimeSpan(element);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                return DeserializeKeyValuePair(element, type);

            if (type.IsValueType)
                return DeserializeValueType(element, type);

            var obj = DeserializeObject(element, type);
            if (obj != null)
            {
                Type interfaceType;
                if ((interfaceType = GetGenericInterface(type, typeof(ICollection<>))) != null)
                {
                    DeserializeICollection(interfaceType, obj, element);
                }
                else if (IsGenericType(type, typeof(Stack<>)))
                {
                    DeserializeStack(type, obj, element);
                }
                else if (IsGenericType(type, typeof(Queue<>)))
                {
                    DeserializeQueue(type, obj, element);
                }
            }

            return obj;
        }

        private void DeserializeICollection(Type interfaceType, object obj, XElement element)
        {
            var expectedItemType = interfaceType.GetGenericArguments()[0];

            interfaceType.GetMethod("Clear").Invoke(obj, null);
            var collectionElement = element.Element(ItemsName);
            foreach (var itemElement in collectionElement.Elements(ItemName))
            {
                var item = DeserializeValue(itemElement, expectedItemType);
                interfaceType.GetMethod("Add").Invoke(obj, new[] { item });
            }
        }

        private void DeserializeStack(Type type, object obj, XElement element)
        {
            var expectedItemType = type.GetGenericArguments()[0];

            type.GetMethod("Clear").Invoke(obj, null);
            var collectionElement = element.Element(ItemsName);
            foreach (var itemElement in collectionElement.Elements(ItemName).Reverse())
            {
                var item = DeserializeValue(itemElement, expectedItemType);
                type.GetMethod("Push").Invoke(obj, new[] { item });
            }
        }

        private void DeserializeQueue(Type type, object obj, XElement element)
        {
            var expectedItemType = type.GetGenericArguments()[0];            

            type.GetMethod("Clear").Invoke(obj, null);
            var collectionElement = element.Element(ItemsName);
            foreach (var itemElement in collectionElement.Elements(ItemName))
            {                
                var item = DeserializeValue(itemElement, expectedItemType);
                type.GetMethod("Enqueue").Invoke(obj, new[] { item });
            }
        }


        private object DeserializeKeyValuePair(XElement element, Type type)
        {
            var types = type.GetGenericArguments();
            var keyValue = DeserializeValue(element.Element("Key"), types[0]);
            var valueValue = DeserializeValue(element.Element("Value"), types[1]);

            var kvp = type.GetConstructor(types).Invoke(new[] { keyValue, valueValue });

            return kvp;
        }

        private object DeserializeArray(XElement element, Type type)
        {
            var items = element.Elements().Where(e => e.Name.LocalName == ItemName).ToArray();
            var elementType = type.GetElementType();
            var upperBound = element.Attribute(DimensionAttribute).Value.Split(',').Select(d => int.Parse(d)).ToArray();

            var obj = Array.CreateInstance(type.GetElementType(), upperBound);

            var index = new int[upperBound.Length];

            var dimension = obj.Rank - 1;
            var i = 0;

            while (dimension >= 0)
            {
                var value = DeserializeValue(items[i], elementType);
                obj.SetValue(value, index);
                i++;

                while (dimension>=0 && ++index[dimension] >= upperBound[dimension])
                {
                    index[dimension] = 0;
                    dimension--;
                }
                if (dimension >= 0)
                    dimension = obj.Rank - 1;
            }
            return obj;
        }

        private string DeserializeString(XElement element)
        {
            return (element.FirstNode as XText)?.Value;
        }

        private DateTime DeserializeDateTime(XElement element)
        {
            return DateTime.ParseExact((element.FirstNode as XText)?.Value, "O", null);
        }

        private TimeSpan DeserializeTimeSpan(XElement element)
        {
            return TimeSpan.ParseExact((element.FirstNode as XText)?.Value, "c", null);
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
