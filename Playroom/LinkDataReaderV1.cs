using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;

namespace Playroom
{
    public class LinkDataReaderV1
    {
        public static string linkAtom;

        public static LinkData ReadXml(XmlReader reader)
        {
            linkAtom = reader.NameTable.Add("Link");

            reader.MoveToContent();
            return ReadLinkElement(reader);
        }

        private static LinkData ReadLinkElement(XmlReader reader)
        {
            LinkData linkData = new LinkData();

            reader.ReadStartElement(linkAtom);
            reader.MoveToContent();

            linkData.LinkedAssetFile = new ParsedPath(reader.ReadElementContentAsString("File", ""), PathType.File);
            reader.MoveToContent();

            reader.ReadEndElement();
            reader.MoveToContent();

            return linkData;
        }
    }
}
