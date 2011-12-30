using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Playroom
{
    public class RectanglesDataReaderV1
    {
        public static string rectanglesAtom;
        public static string classNamesAtom;
        public static string fileNamesAtom;
        public static string platformsAtom;

        public static RectanglesData ReadXml(XmlReader reader)
        {
            rectanglesAtom = reader.NameTable.Add("Rectangles");
            classNamesAtom = reader.NameTable.Add("ClassNames");
            fileNamesAtom = reader.NameTable.Add("FileNames");
            platformsAtom = reader.NameTable.Add("Platforms");

            reader.MoveToContent();
            RectanglesData data = ReadRectanglesXml(reader);
            return data;
        }

        private static RectanglesData ReadRectanglesXml(XmlReader reader)
        {
            RectanglesData data = new RectanglesData();

            reader.ReadStartElement("Rectangles");
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

            platData.FileNames = ReadNamesXml(reader, fileNamesAtom, "FileName");

            reader.ReadEndElement();
            reader.MoveToContent();

            return platData;
        }
    }
}
