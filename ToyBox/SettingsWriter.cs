using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ToyBox
{
    public static class SettingsWriter
    {
        private const int version = 1;

        public static void WriteXml(XmlWriter writer, Dictionary<string, object> settings)
        {
            writer.WriteStartDocument(true);

            writer.WriteStartElement("Settings");

            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Close();
        }
    }
}
