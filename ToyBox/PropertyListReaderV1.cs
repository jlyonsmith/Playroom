using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;

namespace ToyBox
{
    public static class PropertyListReaderV1
    {
        private static string dictAtom;
        private static string arrayAtom;
        private static string keyAtom;
        private static string integerAtom;
        private static string dateAtom;
        private static string plistAtom;

        public static PropertyList ReadXml(XmlReader reader)
        {
            XmlReaderSettings settings = new XmlReaderSettings();

            settings.DtdProcessing = DtdProcessing.Parse;

            Dictionary<string, object> dict;

            dictAtom = reader.NameTable.Add("Dictionary");
            arrayAtom = reader.NameTable.Add("Array");
            keyAtom = reader.NameTable.Add("Key");
            integerAtom = reader.NameTable.Add("Integer");
            dateAtom = reader.NameTable.Add("Date");
            plistAtom = reader.NameTable.Add("PropertyList");

            reader.MoveToContent();
            dict = ReadPropertyList(reader);

            return new PropertyList(dict);
        }

        private static Dictionary<string, object> ReadPropertyList(XmlReader reader)
        {
            reader.ReadStartElement(plistAtom);
            reader.MoveToContent();
            Dictionary<string, object> dict = ReadDict(reader);
            reader.ReadEndElement();
            reader.MoveToContent();

            return dict;
        }

        private static Dictionary<string, object> ReadDict(XmlReader reader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            reader.ReadStartElement(dictAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, dictAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                string key;
                object value;

                ReadKeyValue(reader, out key, out value);
                dict.Add(key, value);
            }

            return dict;
        }

        private static List<object> ReadArray(XmlReader reader)
        {
            List<object> list = new List<object>();

            reader.ReadStartElement(arrayAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, arrayAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                string key;
                object value;

                ReadKeyValue(reader, out key, out value);

                list.Add(value);
            }

            return list;
        }

        private static void ReadKeyValue(XmlReader reader, out string key, out object value)
        {
            key = reader.GetAttribute("Key");

            Type t;

            if (String.ReferenceEquals(reader.Name, dictAtom))
            {
                value = ReadDict(reader);
                return;
            }
            else if (String.ReferenceEquals(reader.Name, arrayAtom))
            {
                value = ReadArray(reader);
                return;
            }
            else if (String.ReferenceEquals(reader.Name, integerAtom))
            {
                t = typeof(int);
            }
            else if (String.ReferenceEquals(reader.Name, dateAtom))
            {
                t = typeof(DateTime);
            }
            else
            {
                t = typeof(string);
            }

            string s = reader.ReadElementContentAsString();
            reader.MoveToContent();

            if (t == typeof(int))
            {
                int result;

                value = (object)(Int32.TryParse(s, out result) ? result : 0);
            }
            else if (t == typeof(DateTime))
            {
                value = (object)(DateTime.Parse(s, null, DateTimeStyles.RoundtripKind));
            }
            else
            {
                value = s;
            }
        }
    }
}
