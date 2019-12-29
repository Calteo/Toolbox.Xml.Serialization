﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Toolbox.Xml.Serialization
{
    public class XmlFormatter<T> where T : class, new()
    {
        public XmlFormatter()
        {
            var attrtibute = typeof(T).GetCustomAttribute<XmlSerializeableAttribute>(true);
            if (attrtibute == null)
                throw new Exception($"missing attribute XmlSerializeable on type '{typeof(T).FullName}'");
        }


        public void Serialize(T obj, string filename, XmlWriterSettings settings = null)
        {
            if (settings == null)
            {
                settings = new XmlWriterSettings
                {
                    CloseOutput = true,
                    Encoding = Encoding.UTF8,
                    NewLineHandling = NewLineHandling.Entitize,
                    Indent = true,
                };
            }
            using (var writer = XmlWriter.Create(filename, settings))
            {
                Serialize(obj, writer);
            }
        }
        
        public void Serialize(T obj, XmlWriter writer)
        {
            writer.WriteStartDocument();

            var name = GetName(typeof(T));
            writer.WriteStartElement(name);

            SerializeObject(obj, writer);

            writer.WriteEndElement();

            writer.WriteEndDocument();
        }

        private void SerializeObject(object obj, XmlWriter writer)
        {
            var properties = GetProperties(obj.GetType());

            foreach (var property in properties)
            {
                writer.WriteStartElement(property.Name);
                if (!property.PropertyType.IsArray)
                {
                    if (CanSerialize(property))
                    {
                        var value = property.GetValue(obj);
                        if (value != null)
                        {
                            var text = Convert.ToString(value, CultureInfo.InvariantCulture);
                            writer.WriteString(text);
                        }
                    }
                    else
                    {
                        var value = property.GetValue(obj);
                        if (value != null)
                        {
                            GetName(value.GetType());           // Just to check the attribute
                            SerializeObject(value, writer);
                        }
                    }
                }
                writer.WriteEndElement();
            }
        }

        private string GetName(Type type)
        {
            var attribute = type.GetCustomAttribute<XmlSerializeableAttribute>();
            if (attribute == null)
                throw new Exception($"missing attribute XmlSerializeable on type {type.FullName}");
            var name = attribute.Name ?? typeof(T).Name;

            return name;
        }

        public T Deserialize(string filename)
        {
            using (var reader = XmlReader.Create(filename))
            {
                return Deserialize(reader);
            }
        }

        public T Deserialize(XmlReader reader)
        {
            var attribute = typeof(T).GetCustomAttribute<XmlSerializeableAttribute>();
            var name = attribute?.Name ?? typeof(T).Name;

            reader.ReadStartElement(name);

            return (T)DeserializeObject(reader, typeof(T));
        }

        private object DeserializeObject(XmlReader reader, Type type) 
        {
            var obj = Activator.CreateInstance(type);
            var properties = new Queue<PropertyInfo>(GetProperties(type));

            var reading = true;

            while (reading && reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            while (properties.Count > 0 && properties.Peek().Name != reader.Name)
                                properties.Dequeue();
                            if (properties.Count > 0)
                            {
                                var property = properties.Dequeue();
                                if (!reader.IsEmptyElement)
                                {
                                    if (!property.PropertyType.IsArray)
                                    {
                                        if (CanSerialize(property))
                                        {
                                            var text = reader.ReadElementContentAsString();
                                            var value = Convert.ChangeType(text, property.PropertyType, CultureInfo.InvariantCulture);
                                            property.SetValue(obj, value);
                                        }
                                        else
                                        {
                                            var value = DeserializeObject(reader, property.PropertyType);
                                            property.SetValue(obj, value);
                                        }
                                    }
                                }
                                else
                                {
                                    property.SetValue(obj, null);
                                }
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        reading = false;
                        break;
                }
            }            

            return obj;
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<XmlNotSerializedAttribute>() == null);
        }

        private bool CanSerialize(PropertyInfo property)
        {
            return property.PropertyType==typeof(string) || !property.PropertyType.IsClass;
        }
    }
}
