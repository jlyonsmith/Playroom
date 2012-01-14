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
        public static string compoundsAtom;

        public static PrismData ReadXml(XmlReader reader)
        {
            prismAtom = reader.NameTable.Add("Prism");
            mappingsAtom = reader.NameTable.Add("Mappings");
            compoundsAtom = reader.NameTable.Add("Compounds");
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

            prismPinboard.Compounds = ReadCompoundsElement(reader);
            prismPinboard.Mappings = ReadMappingsElement(reader);

            reader.ReadEndElement();
            reader.MoveToContent();

            return prismPinboard;
        }

        private static List<PrismCompound> ReadCompoundsElement(XmlReader reader)
        {
            List<PrismCompound> list = new List<PrismCompound>();
            bool empty = reader.IsEmptyElement;

            reader.ReadStartElement(compoundsAtom);
            reader.MoveToContent();

            if (!empty)
            {
                while (true)
                {
                    if (String.ReferenceEquals(reader.Name, compoundsAtom))
                    {
                        reader.ReadEndElement();
                        reader.MoveToContent();
                        break;
                    }

                    list.Add(ReadCompoundElement(reader));
                }
            }

            return list;
        }

        private static PrismCompound ReadCompoundElement(XmlReader reader)
        {
            PrismCompound prismCompound = new PrismCompound();

            prismCompound.LineNumber = ((IXmlLineInfo)reader).LineNumber;

            reader.ReadStartElement("Compound");
            reader.MoveToContent();

            prismCompound.RectangleName = reader.ReadElementContentAsString("Rectangle", "");
            reader.MoveToContent();
            prismCompound.RowCount = reader.ReadElementContentAsInt("Rows", "");
            reader.MoveToContent();
            prismCompound.ColumnCount = reader.ReadElementContentAsInt("Columns", "");
            reader.MoveToContent();
            prismCompound.OutputFileName = new ParsedPath(reader.ReadElementContentAsString("OutputFile", ""), PathType.File);
            reader.MoveToContent();

            reader.ReadEndElement();
            reader.MoveToContent();

            return prismCompound;
        }

        private static List<PrismMapping> ReadMappingsElement(XmlReader reader)
        {
            List<PrismMapping> list = new List<PrismMapping>();

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

            prismMapping.LineNumber = ((IXmlLineInfo)reader).LineNumber;

            reader.ReadStartElement("Mapping");
            reader.MoveToContent();

            bool empty = reader.IsEmptyElement;

            reader.ReadStartElement("Rectangle");

            if (empty)
                prismMapping.RectangleName = null;
            else
            {
                prismMapping.RectangleName = reader.ReadContentAsString();
                reader.ReadEndElement();
            }

            reader.MoveToContent();
            empty = reader.IsEmptyElement;
            reader.ReadStartElement("InputFile", "");

            if (empty)
                prismMapping.InputFileName = null;
            else
            {
                prismMapping.InputFileName = new ParsedPath(reader.ReadContentAsString(), PathType.File);
                reader.ReadEndElement();
            }

            reader.MoveToContent();
            prismMapping.OutputFileName = new ParsedPath(reader.ReadElementContentAsString("OutputFile", ""), PathType.File);
            reader.MoveToContent();

            reader.ReadEndElement();
            reader.MoveToContent();

            return prismMapping;
        }
    }
}
