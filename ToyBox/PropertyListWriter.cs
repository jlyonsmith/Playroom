using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ToyBox
{
    public static class PropertyListWriter
    {
        public static void WriteXml(XmlWriter writer, Dictionary<string, object> dict)
        {
            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.IndentChars = "  ";
            settings.OmitXmlDeclaration = false;
            settings.Encoding = Encoding.UTF8;
            settings.NewLineChars = Environment.NewLine;

            WritePropertyList(writer, dict);
        }

        private static void WritePropertyList(XmlWriter writer, Dictionary<string, object> dict)
        {
            writer.WriteStartElement("PropertyList");
            writer.WriteAttributeString("Version", "1");
            WriteDict(writer, null, dict);
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
            writer.WriteAttributeString("Key", key);

            foreach (var value in list)
            {
                WriteKeyValue(writer, null, value);
            }

            writer.WriteEndElement();
        }
    }
}
