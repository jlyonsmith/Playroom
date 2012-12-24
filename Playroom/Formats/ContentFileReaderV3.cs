using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;
using System.IO;

namespace Playroom
{
    public class ContentFileReaderV3
    {
		private string filePathGroupAtom;
		private string propertyGroupAtom;
		private string targetAtom;
		private string contentAtom;
        private XmlReader reader;

        private ContentFileReaderV3(XmlReader reader)
        {
            filePathGroupAtom = reader.NameTable.Add("FilePathGroup");
            propertyGroupAtom = reader.NameTable.Add("PropertyGroup");
            targetAtom = reader.NameTable.Add("Target");
            contentAtom = reader.NameTable.Add("Content");

            this.reader = reader;
            this.reader.MoveToContent();
        }

        public static ContentFileV3 ReadFile(ParsedPath contentFile)
        {
            using (XmlReader reader = XmlReader.Create(contentFile))
            {
                try
                {
                    return new ContentFileReaderV3(reader).ReadContentElement();
                }
                catch (Exception e)
                {
                    e.Data["LineNumber"] = ((IXmlLineInfo)reader).LineNumber;
                    throw;
                }
            }
        }

        private ContentFileV3 ReadContentElement()
		{
			ContentFileV3 data = new ContentFileV3();

			string version = reader.GetAttribute("Version");

			if (String.IsNullOrEmpty(version))
				throw new XmlException("Version attribute not present");
			
			if (version != "2")
				throw new XmlException("Version attribute must be 2");
			
			reader.ReadStartElement(contentAtom);
			reader.MoveToContent();

			data.FilePaths = new List<ContentFileV3.FilePathGroup>();
			data.Targets = new List<ContentFileV3.Target>();

			while (true)
			{
				if (String.ReferenceEquals(reader.Name, contentAtom))
				{
		            reader.ReadEndElement();
		            reader.MoveToContent();
					break;
				}

				// TODO: Add a global property group too

				if (reader.NodeType == XmlNodeType.Element)
				{
					if (String.ReferenceEquals(reader.Name, filePathGroupAtom))
					{
						List<ContentFileV3.FilePathGroup> pathGroup = ReadFilePathGroupElement();

						data.FilePaths.AddRange(pathGroup);
						continue;
					}
					else if (String.ReferenceEquals(reader.Name, targetAtom))
					{
						ContentFileV3.Target target = ReadTargetElement();

						foreach (var otherTarget in data.Targets)
						{
							if (String.CompareOrdinal(target.Name, otherTarget.Name) == 0)
								throw new XmlException("Duplicate target name '{0}'".CultureFormat(target.Name));
						}

						data.Targets.Add(target);
						continue;
					}
				}

				throw new XmlException("Expected FilePathGroup, PropertyGroup or Target element");
			}

            return data;
        }

        private List<ContentFileV3.FilePathGroup> ReadFilePathGroupElement()
        {
			List<ContentFileV3.FilePathGroup> pathGroups = new List<ContentFileV3.FilePathGroup>();

            // Read outer collection element
            reader.ReadStartElement(filePathGroupAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, filePathGroupAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

				ContentFileV3.FilePathGroup pathGroup = ReadItemElement();

				pathGroups.Add(pathGroup);

				// Deal with an item that has an end element
				if (reader.NodeType == XmlNodeType.EndElement && reader.Name == pathGroup.Name)
				{
					reader.ReadEndElement();
					reader.MoveToContent();
				}
            }

            return pathGroups;
        }

        private ContentFileV3.FilePathGroup ReadItemElement()
        {
			ContentFileV3.FilePathGroup pathGroup = new ContentFileV3.FilePathGroup();
			
			pathGroup.Name = reader.Name;
            pathGroup.Include = reader.GetAttribute("Include");

			if (String.IsNullOrEmpty(pathGroup.Include))
				throw new XmlException("Include attribute must be specified");

            pathGroup.Exclude = reader.GetAttribute("Exclude");

			reader.Skip();
            reader.MoveToContent();

			return pathGroup;
        }

		private ContentFileV3.Target ReadTargetElement()
		{
			ContentFileV3.Target target = new ContentFileV3.Target();

			target.LineNumber = ((IXmlLineInfo)reader).LineNumber;
			target.Name = reader.GetAttribute("Name");

			if (String.IsNullOrWhiteSpace(target.Name))
				throw new XmlException("Target 'Name' attribute must be set");
			
			target.Compiler = reader.GetAttribute("Compiler");

			if (String.IsNullOrWhiteSpace(target.Compiler))
				target.Compiler = String.Empty;

			target.Inputs = reader.GetAttribute("Inputs");

			if (target.Inputs == null)
				target.Inputs = String.Empty;

			target.Inputs = target.Inputs.Trim();

			target.Outputs = reader.GetAttribute("Outputs");

			if (String.IsNullOrWhiteSpace(target.Outputs))
				throw new XmlException("'Outputs' attribute must be set");

			target.Outputs = target.Outputs.Trim();
            
			reader.ReadStartElement();
			reader.MoveToContent();

			// Is there a nested PropertyGroup?
			if (reader.NodeType == XmlNodeType.Element && String.ReferenceEquals(propertyGroupAtom, reader.Name))
			{
				target.Properties = ReadPropertyGroupElement();
			}

			// Is there a separate Target end tag?
			if (reader.NodeType == XmlNodeType.EndElement && String.ReferenceEquals(targetAtom, reader.Name))
			{
				reader.ReadEndElement();
				reader.MoveToContent();
			}

            return target;
        }

        private List<Tuple<string, string>> ReadPropertyGroupElement()
        {
            List<Tuple<string, string>> properties = new List<Tuple<string, string>>();

            reader.ReadStartElement(propertyGroupAtom);
            reader.MoveToContent();

            while (true)
            {
                if (String.ReferenceEquals(reader.Name, propertyGroupAtom))
                {
                    reader.ReadEndElement();
                    reader.MoveToContent();
                    break;
                }

                string key;
                string value;
                
                ReadPropertyElement(out key, out value);

                properties.Add(new Tuple<string, string>(key, value));
            }

            return properties;
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
