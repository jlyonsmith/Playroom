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
        public static string svgDirectoryAtom;
        public static string rowsAtom;
        public static string rowAtom;
        public static string svgFileAtom;

        public static PrismData ReadXml(XmlReader reader)
        {
            prismAtom = reader.NameTable.Add("Prism");
            svgDirectoryAtom = reader.NameTable.Add("SvgDirectory");
            svgFileAtom = reader.NameTable.Add("SvgFile");
            rowsAtom = reader.NameTable.Add("Rows");
            rowAtom = reader.NameTable.Add("Row");
            
            reader.MoveToContent();
            return ReadPrismElement(reader);
        }

        private static PrismData ReadPrismElement(XmlReader reader)
        {
            PrismData prismData = new PrismData();

            reader.ReadStartElement("Prism");
            reader.MoveToContent();

            prismData.PinboardFile = new ParsedPath(reader.ReadElementContentAsString("PinboardFile", ""), PathType.File);
            reader.MoveToContent();
            prismData.RectangleName = reader.ReadElementContentAsString("Rectangle", "");
            reader.MoveToContent();

            if (reader.NodeType == XmlNodeType.Element && String.ReferenceEquals(svgDirectoryAtom, reader.Name))
            {
                prismData.SvgDirectory = new ParsedPath(reader.ReadElementContentAsString("SvgDirectory", ""), PathType.Directory);
                reader.MoveToContent();
            }

            if (reader.NodeType == XmlNodeType.Element && String.ReferenceEquals(reader.Name, svgFileAtom))
            {
                prismData.SvgFiles = new List<List<ParsedPath>>();
                prismData.SvgFiles.Add(new List<ParsedPath>());
                prismData.SvgFiles[0].Add(new ParsedPath(reader.ReadElementContentAsString(svgFileAtom, ""), PathType.File));
                reader.MoveToContent();
            }
            else
            {
                prismData.SvgFiles = ReadRowsElement(reader);
            }

            reader.ReadEndElement();

            return prismData;
        }

        private static List<List<ParsedPath>> ReadRowsElement(XmlReader reader)
        {
            List<List<ParsedPath>> rows = new List<List<ParsedPath>>();

            reader.ReadStartElement(rowsAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, rowsAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                rows.Add(ReadRowElement(reader));
            }

            return rows;
        }

        private static List<ParsedPath> ReadRowElement(XmlReader reader)
        {
            List<ParsedPath> row = new List<ParsedPath>();

            reader.ReadStartElement(rowAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, rowAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                row.Add(new ParsedPath(reader.ReadElementContentAsString(svgFileAtom, ""), PathType.File));
                reader.MoveToContent();
            }

            return row;
        }
    }
}
