using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Playroom.Compilers;
using ToolBelt;
using System.IO;
using System.Xml;

namespace Playroom.Converters
{
    public class PinboardToCsCompiler : IContentCompiler
    {
        #region Classes
        private class RectangleData
        {
            public class Class
            {
                public string ClassNamePrefix { get; set; }
                public List<string> RectangleNames { get; set; }
            }

            public string Namespace { get; set; }
            public List<Class> Classes { get; set; }
        }

        #endregion
        
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".pinboard" }; } }

        public string[] OutputExtensions { get { return new string[] { ".cs" }; } }

        public void Compile(BuildContext buildContext, BuildItem buildItem)
        {
            List<PinboardData> inputPinboards = new List<PinboardData>();

            foreach (var pinboardFile in buildItem.InputFiles)
            {
                buildContext.Output.Message(MessageImportance.Low, "Reading pinboard file '{0}'", pinboardFile);

                PinboardData pinboardData = ReadPinboardData(pinboardFile);

                if (pinboardData == null)
                {
                    buildContext.Output.Error("Unable to read pinboard file '{0}'", pinboardFile);
                    return;
                }

                inputPinboards.Add(pinboardData);
            }

            TextWriter writer;
            bool closeWriter = true;
            ParsedPath csFile = buildItem.OutputFiles[0];

            if (!String.IsNullOrEmpty(csFile))
            {
                buildContext.Output.Message(MessageImportance.Normal, "Writing output file '{0}'", csFile);

                writer = new StreamWriter(csFile, false, Encoding.UTF8);
            }
            else
            {
                writer = Console.Out;
                closeWriter = false;
            }

            RectangleData rectangleData = null;

            WriteCsOutput(writer, rectangleData);

            if (closeWriter)
            {
                writer.Close();
            }
        }

        #endregion

        #region Methods
        private void WriteCsOutput(TextWriter writer, RectangleData rectangleData)
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using Microsoft.Xna.Framework;");
            writer.WriteLine("using Microsoft.Xna.Framework.Graphics;");
            writer.WriteLine("using System.Text;");
            writer.WriteLine("");
            writer.WriteLine("namespace {0}", rectangleData.Namespace);
            writer.WriteLine("{");

            for (int i = 0; i < rectangleData.Classes.Count; i++)
            {
                RectangleData.Class classData = rectangleData.Classes[i];

                writer.WriteLine("\tpublic class {0}Rectangles", classData.ClassNamePrefix);
                writer.WriteLine("\t{");

                writer.WriteLine("\t\tprivate Rectangle[] rectangles;");
                writer.WriteLine();
                writer.WriteLine("\t\tpublic {0}Rectangles(Rectangle[] rectangles)", classData.ClassNamePrefix);
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tthis.rectangles = rectangles;");
                writer.WriteLine("\t\t}");
                writer.WriteLine();

                for (int j = 0; j < classData.RectangleNames.Count; j++)
                {
                    writer.WriteLine("\t\tpublic Rectangle {0} {{ get {{ return rectangles[{1}]; }} }}",
                        classData.RectangleNames[j],
                        j);
                }

                writer.WriteLine("\t}");
                writer.WriteLine();
            }

            writer.WriteLine("}");
        }

        public static PinboardData ReadPinboardData(ParsedPath pinboardPath)
        {
            PinboardData pinboardData = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(pinboardPath))
                {
                    pinboardData = PinboardDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception e)
            {
                if (!(e is XmlException || e is IOException))
                    throw;
            }

            return pinboardData;
        }
        #endregion
    }
}
