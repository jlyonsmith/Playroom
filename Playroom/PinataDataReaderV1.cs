using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;

namespace Playroom
{
    public class PinataDataReaderV1
    {
        public static string pinataAtom;
        public static string classesAtom;

        public static PinataData ReadXml(XmlReader reader)
        {
            pinataAtom = reader.NameTable.Add("Pinata");
            classesAtom = reader.NameTable.Add("Classes");

            reader.MoveToContent();

            return ReadPinataElement(reader);
        }

        public static PinataData ReadPinataElement(XmlReader reader)
        {
            PinataData data = new PinataData();

            reader.ReadStartElement(pinataAtom);
            reader.MoveToContent();

            data.Namespace = reader.ReadElementContentAsString("Namespace", "");
            reader.MoveToContent();

            data.Classes = ReadClassesElement(reader);

            reader.ReadEndElement();
            reader.MoveToContent();

            return data;
        }

        private static List<PinataClassData> ReadClassesElement(XmlReader reader)
        {
            List<PinataClassData> list = new List<PinataClassData>();

            // Read outer collection element
            reader.ReadStartElement(classesAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, classesAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                PinataClassData classData = ReadClassElement(reader);

                list.Add(classData);
            }

            return list;
        }

        private static PinataClassData ReadClassElement(XmlReader reader)
        {
            PinataClassData classData = new PinataClassData();

            reader.ReadStartElement("Class");
            reader.MoveToContent();

            classData.Prefix = reader.ReadElementContentAsString("Prefix", "");
            reader.MoveToContent();

            classData.PinboardFile = new ParsedPath(reader.ReadElementContentAsString("Pinboard", ""), PathType.File);
            reader.MoveToContent();

            reader.ReadEndElement();
            reader.MoveToContent();

            return classData;
        }
    }
}
