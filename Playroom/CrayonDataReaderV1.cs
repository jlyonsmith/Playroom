using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Playroom
{
    public class CrayonDataReaderV1
    {
        public static string crayonAtom;
        public static string classNamesAtom;
        public static string filesAtom;
        public static string platformsAtom;

        public static CrayonData ReadXml(XmlReader reader)
        {
            crayonAtom = reader.NameTable.Add("Crayon");
            classNamesAtom = reader.NameTable.Add("ClassNames");
            filesAtom = reader.NameTable.Add("Files");
            platformsAtom = reader.NameTable.Add("Platforms");

            reader.MoveToContent();
            CrayonData data = ReadRectanglesXml(reader);
            return data;
        }

        private static CrayonData ReadRectanglesXml(XmlReader reader)
        {
            CrayonData data = new CrayonData();

            reader.ReadStartElement(crayonAtom);
            reader.MoveToContent();

            data.Namespace = reader.ReadElementContentAsString("Namespace", "");
            reader.MoveToContent();

            data.ClassNames = ReadNamesXml(reader, classNamesAtom, "ClassName");
            data.Platforms = ReadPlatformsXml(reader);

            reader.ReadEndElement();
            reader.MoveToContent();

            return data;
        }

        private static List<string> ReadNamesXml(XmlReader reader, string collectionName, string itemName)
        {
            List<string> list = new List<string>();

            // Read outer collection element
            reader.ReadStartElement(collectionName);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, collectionName))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                string className = reader.ReadElementContentAsString(itemName, "");
                reader.MoveToContent();

                list.Add(className);
            }

            return list;
        }

        private static List<PlatformData> ReadPlatformsXml(XmlReader reader)
        {
            List<PlatformData> list = new List<PlatformData>();

            // Read outer <Platforms>
            reader.ReadStartElement(platformsAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, platformsAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                PlatformData platData = ReadPlatformData(reader);
                    
                list.Add(platData);
            }

            return list;
        }

        private static PlatformData ReadPlatformData(XmlReader reader)
        {
            PlatformData platData = new PlatformData();

            reader.ReadStartElement("Platform");
            reader.MoveToContent();

            platData.Symbol = reader.ReadElementContentAsString("Symbol", "");
            reader.MoveToContent();

            platData.FileNames = ReadNamesXml(reader, filesAtom, "File");

            reader.ReadEndElement();
            reader.MoveToContent();

            return platData;
        }
    }
}
