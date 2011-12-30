using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ToyBox
{
    public static class SettingsReaderV1
    {
        private static string settingsAtom;

        public static Dictionary<string, object> ReadXml(XmlReader reader)
        {
            settingsAtom = reader.NameTable.Add("Settings");

            Dictionary<string, object> settings = new Dictionary<string, object>();

            return settings;
        }
    }
}
