using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;

namespace Playroom
{
    public class PrismDataReaderV1
    {
        public static string prismAtom;
        public static string mappingsAtom;
        public static string pinboardsAtom;

        public static PrismData ReadXml(XmlReader reader)
        {
            prismAtom = reader.NameTable.Add("Prism");
            mappingsAtom = reader.NameTable.Add("Mappings");
            pinboardsAtom = reader.NameTable.Add("Pinboards");

            reader.MoveToContent();
            
            return ReadPrismElement(reader);
        }

        private static PrismData ReadPrismElement(XmlReader reader)
        {
            PrismData prismData = new PrismData();

            reader.ReadStartElement("Prism");
            reader.MoveToContent();

            prismData.Pinboards = ReadPinboardsElement(reader);

            reader.ReadEndElement();

            return prismData;
        }

        private static List<PrismPinboard> ReadPinboardsElement(XmlReader reader)
        {
            List<PrismPinboard> list = new List<PrismPinboard>();

            // Read outer collection element
            reader.ReadStartElement(pinboardsAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, pinboardsAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                PrismPinboard prismPinboard = ReadPinboardElement(reader);

                list.Add(prismPinboard);
            }

            return list;
        }

        private static PrismPinboard ReadPinboardElement(XmlReader reader)
        {
            PrismPinboard prismPinboard = new PrismPinboard();

            reader.ReadStartElement("Pinboard");
            reader.MoveToContent();

            prismPinboard.FileName = new ParsedPath(reader.ReadElementContentAsString("File", ""), PathType.File);
            reader.MoveToContent();

            prismPinboard.Mappings = ReadMappingsElement(reader);

            reader.ReadEndElement();
            reader.MoveToContent();

            return prismPinboard;
        }

        private static List<PrismMapping> ReadMappingsElement(XmlReader reader)
        {
            List<PrismMapping> list = new List<PrismMapping>();

            // Read outer <Platforms>
            reader.ReadStartElement(mappingsAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, mappingsAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                PrismMapping prismMapping = ReadMappingElement(reader);

                list.Add(prismMapping);
            }

            return list;
        }

        private static PrismMapping ReadMappingElement(XmlReader reader)
        {
            PrismMapping prismMapping = new PrismMapping();

            reader.ReadStartElement("Mapping");
            reader.MoveToContent();

            prismMapping.RectangleName = reader.ReadElementContentAsString("Rectangle", "");
            reader.MoveToContent();

            bool empty = reader.IsEmptyElement;
            
            reader.ReadStartElement("SvgFile", "");

            if (empty)
                prismMapping.SvgFileName = null;
            else
            {
                prismMapping.SvgFileName = new ParsedPath(reader.ReadContentAsString(), PathType.File);
                reader.ReadEndElement();
            }

            reader.MoveToContent();
            prismMapping.PngFileName = new ParsedPath(reader.ReadElementContentAsString("PngFile", ""), PathType.File);
            reader.MoveToContent();

            reader.ReadEndElement();
            reader.MoveToContent();

            return prismMapping;
        }
    }
}
