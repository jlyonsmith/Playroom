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
        public static string converterAtom;
        public static string rowsAtom;
        public static string rowAtom;
        public static string svgFileAtom;
        public static string rectangleAtom;
        public static string pinboardFileAtom;

        public static PrismData ReadXml(XmlReader reader)
        {
            prismAtom = reader.NameTable.Add("Prism");
            svgDirectoryAtom = reader.NameTable.Add("SvgDirectory");
            converterAtom = reader.NameTable.Add("Converter");
            svgFileAtom = reader.NameTable.Add("SvgFile");
            rowsAtom = reader.NameTable.Add("Rows");
            rowAtom = reader.NameTable.Add("Row");
            rectangleAtom = reader.NameTable.Add("Rectangle");
            pinboardFileAtom = reader.NameTable.Add("PinboardFile");
            
            reader.MoveToContent();
            return ReadPrismElement(reader);
        }

        private static PrismData ReadPrismElement(XmlReader reader)
        {
            PrismData prismData = new PrismData();

            reader.ReadStartElement("Prism");
            reader.MoveToContent();

            bool readConverter = false;

            while (true)
            {
                if (reader.NodeType == XmlNodeType.EndElement && String.ReferenceEquals(prismAtom, reader.Name))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                if (reader.NodeType != XmlNodeType.Element)
                    throw new XmlException(PlayroomResources.ElementNodeExpected);

                if (String.ReferenceEquals(pinboardFileAtom, reader.Name))
                {
                    if (prismData.PinboardFile != null)
                        throw new XmlException(PlayroomResources.DuplicateElement(pinboardFileAtom));

                    prismData.PinboardFile = new ParsedPath(reader.ReadElementContentAsString(pinboardFileAtom, ""), PathType.File);
                    reader.MoveToContent();
                }
                else if (String.ReferenceEquals(rectangleAtom, reader.Name))
                {
                    if (prismData.RectangleName != null)
                        throw new XmlException(PlayroomResources.DuplicateElement(rectangleAtom));

                    prismData.RectangleName = reader.ReadElementContentAsString(rectangleAtom, "");
                    reader.MoveToContent();
                }
                else if (String.ReferenceEquals(converterAtom, reader.Name))
                {
                    if (readConverter)
                        throw new XmlException(PlayroomResources.DuplicateElement(converterAtom));

                    prismData.Converter = (SvgToPngConverter)Enum.Parse(
                        typeof(SvgToPngConverter), reader.ReadElementContentAsString(converterAtom, ""));
                    readConverter = true;
                    reader.MoveToContent();
                }
                else if (String.ReferenceEquals(svgDirectoryAtom, reader.Name))
                {
                    if (prismData.SvgDirectory != null)
                        throw new XmlException(PlayroomResources.DuplicateElement(svgDirectoryAtom));

                    prismData.SvgDirectory = new ParsedPath(reader.ReadElementContentAsString(svgDirectoryAtom, ""), PathType.Directory);
                    reader.MoveToContent();
                }
                else if (String.ReferenceEquals(reader.Name, svgFileAtom))
                {
                    if (prismData.SvgFiles != null)
                        throw new XmlException(PlayroomResources.DuplicateElement(svgFileAtom));

                    prismData.SvgFiles = new List<List<ParsedPath>>();
                    prismData.SvgFiles.Add(new List<ParsedPath>());
                    prismData.SvgFiles[0].Add(new ParsedPath(reader.ReadElementContentAsString(svgFileAtom, ""), PathType.File));
                    reader.MoveToContent();
                }
                else
                {
                    if (prismData.SvgFiles != null)
                        throw new XmlException(PlayroomResources.DuplicateElement(rowsAtom));

                    prismData.SvgFiles = ReadRowsElement(reader);
                }
            }

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
