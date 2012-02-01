using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace ToyBox
{
    public static class PropertyListWriter
    {
        public static void WriteXml(XmlWriter writer, PropertyList propList)
        {
            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.OmitXmlDeclaration = false;
            settings.Encoding = Encoding.UTF8;
            settings.NewLineChars = Environment.NewLine;

            WritePropertyList(writer, propList);
        }

        private static void WritePropertyList(XmlWriter writer, PropertyList propList)
        {
            writer.WriteStartElement("PropertyList");
            writer.WriteAttributeString("Version", "1");

            FieldInfo dictField = propList.GetType().GetField("dict", 
                System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance);

            WriteDict(writer, null, (Dictionary<string, object>)dictField.GetValue(propList));
            writer.WriteEndElement();
        }

        private static void WriteKeyValue(XmlWriter writer, string key, object value)
        {
            if (value.GetType() == typeof(string))
            {
                writer.WriteStartElement("String");
                if (key != null)
                    writer.WriteAttributeString("Key", key);
                writer.WriteString((string)value);
                writer.WriteEndElement();
            }
            else if (value.GetType() == typeof(bool))
            {
                writer.WriteStartElement("Boolean");
                if (key != null)
                    writer.WriteAttributeString("Key", key);
                writer.WriteString(value.ToString());
                writer.WriteEndElement();
            }
            else if (value.GetType() == typeof(DateTime))
            {
                writer.WriteStartElement("Date");
                if (key != null)
                    writer.WriteAttributeString("Key", key);
                writer.WriteString(((DateTime)value).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
                writer.WriteEndElement();
            }
            else if (value.GetType() == typeof(int))
            {
                writer.WriteStartElement("Integer");
                if (key != null)
                    writer.WriteAttributeString("Key", key);
                writer.WriteString(((int)value).ToString());
                writer.WriteEndElement();
            }
            else if (value.GetType() == typeof(List<object>))
            {
                WriteArray(writer, key, (List<object>)value);
            }
            else if (value.GetType() == typeof(Dictionary<string, object>))
            {
                WriteDict(writer, key, (Dictionary<string, object>)value);
            }
        }

        private static void WriteDict(XmlWriter writer, string key, Dictionary<string, object> dict)
        {
            writer.WriteStartElement("Dictionary");
            
            if (key != null)
                writer.WriteAttributeString("Key", key);

            foreach (var pair in dict)
            {
                WriteKeyValue(writer, pair.Key, pair.Value);
            }

            writer.WriteEndElement();
        }

        private static void WriteArray(XmlWriter writer, string key, List<object> list)
        {
            writer.WriteStartElement("Array");

            if (key != null)
                writer.WriteAttributeString("Key", key);

            foreach (var value in list)
            {
                WriteKeyValue(writer, null, value);
            }

            writer.WriteEndElement();
        }
    }
}
