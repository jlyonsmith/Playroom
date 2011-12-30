using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ToolBelt;
using Playroom;
using System.Xml;

namespace Crayon
{
    class Program
    {
        public static int Main(string[] args)
        {
            CrayonTool tool = new CrayonTool(new ConsoleOutputter());

            if (!((IProcessCommandLine)tool).ProcessCommandLine(args))
                return 1;

            try
            {
                tool.Execute();
            }
            catch (CommandLineArgumentException e)
            {
                tool.Output.Error(e.Message);
            }

            return tool.Output.HasOutputErrors ? 1 : 0;
        }
    }

    [CommandLineDescription("Pinboard to code converter")]
    [CommandLineTitle("Crayon Tool")]
    public class CrayonTool : IProcessCommandLine
    {
        [DefaultCommandLineArgument("default", Description = "Rectangles data file", ValueHint = "<rectangles-file>")]
        public ParsedPath RectanglesFile { get; set; }

        [CommandLineArgument("out", ShortName="o", Description = "Output file", ValueHint = "<out-file>")]
        public ParsedPath OutputFile { get; set; }

        [CommandLineArgument("help", Description = "Displays this help", ShortName = "?")]
        public bool ShowHelp { get; set; }

        public OutputHelper Output { get; set; }

        private CommandLineParser parser;

        private CommandLineParser Parser
        {
            get
            {
                if (parser == null)
                    parser = new CommandLineParser(this.GetType());

                return parser;
            }
        }

        public CrayonTool(IOutputter outputter)
        {
            this.Output = new OutputHelper(outputter);
        }

        public void Execute()
        {
            Console.WriteLine(Parser.LogoBanner);

            if (ShowHelp)
            {
                Console.WriteLine(Parser.Usage);
                return;
            }

            if (String.IsNullOrEmpty(RectanglesFile))
            {
                Output.Error("A rectangles file must be specified");
                return;
            }

            Output.Message(MessageImportance.Normal, "Reading rectangles file '{0}'", this.RectanglesFile);

            RectanglesData rectData = ReadRectanglesData(this.RectanglesFile);

            Output.Message(MessageImportance.Low, "{0} class, {1} platforms", rectData.ClassNames.Count, rectData.Platforms.Count);

            foreach (var platformData in rectData.Platforms)
            {
                if (platformData.FileNames.Count != rectData.ClassNames.Count)
                {
                    Output.Error("Insufficient pinboard files specified for platform {0}. There must be one per class, in the same order.", 
                        platformData.Symbol);
                    return;
                }

                platformData.Pinboards = new List<PinboardData>();

                foreach (var fileName in platformData.FileNames)
                {
                    Output.Message(MessageImportance.Low, "Reading pinboard file '{0}'", fileName);

                    PinboardData pinData = ReadPinboardData(fileName);

                    if (pinData == null)
                    {
                        return;
                    }

                    platformData.Pinboards.Add(pinData);
                }
            }

            TextWriter writer;
            bool closeWriter = true;

            if (!String.IsNullOrEmpty(OutputFile))
            {
                Output.Message(MessageImportance.Normal, "Writing output file '{0}'", OutputFile);

                writer = new StreamWriter(OutputFile, false, Encoding.UTF8);
            }
            else
            {
                writer = Console.Out;
                closeWriter = false;
            }

            WriteCsOutput(writer, rectData);

            if (closeWriter)
            {
                writer.Close();
            }
        }

        private PinboardData ReadPinboardData(string fileName)
        {
            PinboardData data = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(fileName))
                {
                    data = PinboardDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is XmlException || ex is FormatException))
                    throw;

                Output.Error("Unable to read Pinboard file '{0}'", fileName);
                return null;
            }

            return data;
        }

        private RectanglesData ReadRectanglesData(string fileName)
        {
            RectanglesData data = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(fileName))
                {
                    data = RectanglesDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is XmlException || ex is FormatException))
                    throw;

                Output.Error("Unable to read Pinboard file '{0}'", fileName);
                return null;
            }

            return data;
        }

        private void WriteCsOutput(TextWriter writer, RectanglesData rectData)
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using Microsoft.Xna.Framework;");
            writer.WriteLine("using Microsoft.Xna.Framework.Audio;");
            writer.WriteLine("using Microsoft.Xna.Framework.Content;");
            writer.WriteLine("using Microsoft.Xna.Framework.GamerServices;");
            writer.WriteLine("using Microsoft.Xna.Framework.Graphics;");
            writer.WriteLine("using Microsoft.Xna.Framework.Input;");
            writer.WriteLine("using Microsoft.Xna.Framework.Media;");
            writer.WriteLine("using System.Linq;");
            writer.WriteLine("using System.Text;");
            writer.WriteLine("");
            writer.WriteLine("namespace {0}", rectData.Namespace);
            writer.WriteLine("{");

            for (int i = 0; i < rectData.Platforms.Count; i++)
            {
                PlatformData platData = rectData.Platforms[i];

                writer.WriteLine("#if {0}", platData.Symbol);

                for (int j = 0; j < rectData.ClassNames.Count; j++)
                {
                    writer.WriteLine("\tpublic static class {0}Rectangles", rectData.ClassNames[j]);
                    writer.WriteLine("\t{");

                    PinboardData pinData = platData.Pinboards[j];

                    WriteRectInfo(writer, pinData.ScreenRectInfo);

                    foreach (var rectInfo in pinData.RectInfos)
                    {
                        WriteRectInfo(writer, rectInfo);
                    }
                }

                writer.WriteLine("\t}");
                writer.WriteLine("#endif");
            }

            writer.WriteLine("}");
        }

        private void WriteRectInfo(TextWriter writer, RectangleInfo rectInfo)
        {
            writer.WriteLine("\t\tpublic static Rectangle {0} = new Rectangle({1}, {2}, {3}, {4});",
                rectInfo.Name,
                rectInfo.X,
                rectInfo.Y,
                rectInfo.Width,
                rectInfo.Height);
        }

        #region IProcessCommandLine Members

        public bool ProcessCommandLine(string[] args)
        {
            try
            {
                Parser.ParseAndSetTarget(args, this);
            }
            catch (CommandLineArgumentException e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        #endregion
    }
}
