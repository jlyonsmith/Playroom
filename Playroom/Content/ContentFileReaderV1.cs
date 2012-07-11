using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;
using System.IO;

namespace Playroom
{
    public class ContentFileReaderV1
    {
        private string contentAtom;
        private string compilerAssemblyFilesAtom;
        private string itemsAtom;
        private string inputFilesAtom;
        private string outputFilesAtom;
        private string propertiesAtom;
        private XmlReader reader;

        private ContentFileReaderV1(XmlReader reader)
        {
            contentAtom = reader.NameTable.Add("Content");
            compilerAssemblyFilesAtom = reader.NameTable.Add("CompilerAssemblyFiles");
            itemsAtom = reader.NameTable.Add("Items");
            inputFilesAtom = reader.NameTable.Add("InputFiles");
            outputFilesAtom = reader.NameTable.Add("OutputFiles");
            propertiesAtom = reader.NameTable.Add("Properties");

            this.reader = reader;
            this.reader.MoveToContent();
        }

        public static ContentFileV1 ReadFile(ParsedPath contentFile)
        {
            using (XmlReader reader = XmlReader.Create(contentFile))
            {
                try
                {
                    return new ContentFileReaderV1(reader).ReadContentElement();
                }
                catch (Exception e)
                {
                    e.Data["LineNumber"] = ((IXmlLineInfo)reader).LineNumber;
                    throw;
                }
            }
        }

        private ContentFileV1 ReadContentElement()
        {
            ContentFileV1 data = new ContentFileV1();

            reader.ReadStartElement(contentAtom);
            reader.MoveToContent();

            data.CompilerAssemblyFiles = ReadCompilerAssemblyFilesElement();
            data.Items = ReadItemsElement();

            reader.ReadEndElement();
            reader.MoveToContent();

            return data;
        }

        private List<string> ReadCompilerAssemblyFilesElement()
        {
            List<string> list = new List<string>();

            // Read outer collection element
            reader.ReadStartElement(compilerAssemblyFilesAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, compilerAssemblyFilesAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                string assemblyFile = reader.ReadElementContentAsString("File", "");
                reader.MoveToContent();

                list.Add(assemblyFile);
            }

            return list;
        }

        private List<ContentFileV1.Item> ReadItemsElement()
        {
            List<ContentFileV1.Item> list = new List<ContentFileV1.Item>();

            // Read outer collection element
            reader.ReadStartElement(itemsAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, itemsAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                ContentFileV1.Item itemData = ReadItemElement();

                list.Add(itemData);
            }

            return list;
        }

        private ContentFileV1.Item ReadItemElement()
        {
            ContentFileV1.Item item = new ContentFileV1.Item();

            item.LineNumber = ((IXmlLineInfo)reader).LineNumber;
            
            reader.ReadStartElement("Item");
            reader.MoveToContent();

            item.InputFiles = ReadInputFilesElement();
            item.OutputFiles = ReadOutputFilesElement();
            item.Properties = ReadPropertiesElement();

            reader.ReadEndElement();
            reader.MoveToContent();

            return item;
        }

        private List<string> ReadInputFilesElement()
        {
            List<string> list = new List<string>();

            reader.ReadStartElement(inputFilesAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, inputFilesAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                string inputFile = reader.ReadElementContentAsString("File", "");
                reader.MoveToContent();

                list.Add(inputFile);
            }

            return list;
        }

        private List<string> ReadOutputFilesElement()
        {
            List<string> list = new List<string>();

            reader.ReadStartElement(outputFilesAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, outputFilesAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                string outputFile = reader.ReadElementContentAsString("File", "");
                reader.MoveToContent();

                list.Add(outputFile);
            }

            return list;
        }

        private List<Tuple<string, string>> ReadPropertiesElement()
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();

            // This element is optional
            if (!reader.IsStartElement(propertiesAtom))
                return list;

            // Read outer collection element
            reader.ReadStartElement(propertiesAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, propertiesAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                string key;
                string value;
                
                ReadPropertyElement(out key, out value);

                list.Add(new Tuple<string, string>(key, value));
            }

            return list;
        }

        private void ReadPropertyElement(out string key, out string value)
        {
            key = reader.Name;
            reader.MoveToContent();

            value = reader.ReadElementContentAsString();
            reader.MoveToContent();
        }
    }
}
