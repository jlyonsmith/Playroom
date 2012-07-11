using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.Resources;
using System.Collections;
using System.ComponentModel.Design;
using System.Xml;

namespace Playroom
{
    public class ResxToStringsCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".resx" }; }
        }

        public string[] OutputExtensions
        {
            get { return new string[] { ".strings" }; }
        }

        public BuildContext Context { get; set; }
        public BuildItem Item { get; set; }

        public void Compile()
        {
#if WINDOWS
            ParsedPath resxFile = Item.InputFiles.Where(f => f.Extension == ".resx").First();
            ParsedPath stringsFile = Item.OutputFiles.Where(f => f.Extension == ".strings").First();

            using (ResXResourceReader resxReader = new ResXResourceReader(resxFile))
            {
                XmlWriterSettings xmlSettings = new XmlWriterSettings();

                xmlSettings.Indent = true;
                xmlSettings.IndentChars = "\t";

                using (XmlWriter xmlWriter = XmlWriter.Create(stringsFile, xmlSettings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("Strings");

                    ITypeResolutionService typeResolver = null;
                
                    resxReader.UseResXDataNodes = true;

                    foreach (DictionaryEntry entry in resxReader)
                    {
                        ResXDataNode dataNode = entry.Value as ResXDataNode;

                        string valueTypeName = dataNode.GetValueTypeName(typeResolver);

                        if (valueTypeName.StartsWith("System.String"))
                        {
                            string value = (string)dataNode.GetValue(typeResolver);

                            xmlWriter.WriteStartElement("String");
                            xmlWriter.WriteAttributeString("Name", dataNode.Name);
                            xmlWriter.WriteString(value);
                            xmlWriter.WriteEndElement();
                        }
                    }

                    xmlWriter.WriteEndElement();
                }
            }
#endif // WINDOWS
        }

        #endregion
    }
}
