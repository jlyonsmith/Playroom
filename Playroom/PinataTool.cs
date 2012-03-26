using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.IO;
using System.Xml;

namespace Playroom
{
    [CommandLineDescription("Pinboard to code converter")]
    [CommandLineTitle("Pinata Tool")]
    public class PinataTool : ITool, IProcessCommandLine
    {
        private bool runningFromCommandLine = false;
    
        [DefaultCommandLineArgument("default", Description = "Input Pinata data file", ValueHint = "<pinata-file>")]
        public ParsedPath PinataFile { get; set; }

        [CommandLineArgument("csfile", ShortName = "cs", Description = "Output C# file", ValueHint = "<cs-file>")]
        public ParsedPath CsFile { get; set; }

        [CommandLineArgument("help", Description = "Displays this help", ShortName = "?")]
        public bool ShowHelp { get; set; }

        [CommandLineArgument("nologo", Description = "Suppress logo banner")]
        public bool NoLogo { get; set; }

        [CommandLineArgument("rebuild", Description = "Force a rebuild even if all files are up-to-date")]
        public bool Rebuild { get; set; }

        public OutputHelper Output { get; set; }

        private CommandLineParser parser;

        public CommandLineParser Parser
        {
            get
            {
                if (parser == null)
                    parser = new CommandLineParser(this.GetType());

                return parser;
            }
        }

        public PinataTool(IOutputter outputter)
        {
            this.Output = new OutputHelper(outputter);
        }

        public void Execute()
        {
            if (!NoLogo)
                Console.WriteLine(Parser.LogoBanner);

            if (!runningFromCommandLine)
            {
                Parser.GetTargetArguments(this);
                Output.Message(MessageImportance.Normal, Parser.CommandName + Parser.Arguments);
            }

            if (ShowHelp)
            {
                Console.WriteLine(Parser.Usage);
                return;
            }

            if (String.IsNullOrEmpty(PinataFile))
            {
                Output.Error("A .pinata file must be specified");
                return;
            }

            this.PinataFile = this.PinataFile.MakeFullPath();

            if (String.IsNullOrEmpty(CsFile))
            {
                Output.Error("A .cs file must be specified");
                return;
            }

            this.CsFile = this.CsFile.MakeFullPath();

            Output.Message(MessageImportance.Low, "Reading Pinata file '{0}'", this.PinataFile);

            PinataData pinataData = ReadPinataData(this.PinataFile);

            if (pinataData == null)
                return;

            Output.Message(MessageImportance.Low, "{0} classes read", pinataData.Classes.Count);

            foreach (var classData in pinataData.Classes)
            {
                classData.PinboardFile = classData.PinboardFile.MakeFullPath(this.PinataFile);
            }

            // Check dates to see if a rebuild is required
            bool doCompile = Rebuild;

            if (!doCompile)
            {
                DateTime outputFileWriteTime = File.GetLastWriteTime(CsFile);
                
                doCompile = (File.GetLastWriteTime(this.PinataFile) > outputFileWriteTime);

                if (!doCompile)
                {
                    foreach (var classData in pinataData.Classes)
                    {
                        if (File.GetLastWriteTime(classData.PinboardFile) > outputFileWriteTime)
                        {
                            doCompile = true;
                            break;
                        }
                    }
                }
            }

            if (!doCompile)
            {
                Output.Message(MessageImportance.Low, "All files up-to-date");
                return;
            }

            foreach (var classData in pinataData.Classes)
            {
                Output.Message(MessageImportance.Low, "Reading pinboard file '{0}'", classData.PinboardFile);

                classData.Pinboard = ReadPinboardData(classData.PinboardFile);

                if (classData.Pinboard == null)
                    return;
            }

            TextWriter writer;
            bool closeWriter = true;

            if (!String.IsNullOrEmpty(CsFile))
            {
                Output.Message(MessageImportance.Normal, "Writing output file '{0}'", CsFile);

                writer = new StreamWriter(CsFile, false, Encoding.UTF8);
            }
            else
            {
                writer = Console.Out;
                closeWriter = false;
            }

            WriteCsOutput(writer, pinataData);

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

        private PinataData ReadPinataData(string fileName)
        {
            PinataData data = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(fileName))
                {
                    data = PinataDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception ex)
            {
                Output.Error("Unable to read Pinata file '{0}': {1}", fileName, ex.Message);
                return null;
            }

            return data;
        }

        private void WriteCsOutput(TextWriter writer, PinataData pinataData)
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using Microsoft.Xna.Framework;");
            writer.WriteLine("using Microsoft.Xna.Framework.Graphics;");
            writer.WriteLine("using System.Text;");
            writer.WriteLine("");
            writer.WriteLine("namespace {0}", pinataData.Namespace);
            writer.WriteLine("{");

            for (int i = 0; i < pinataData.Classes.Count; i++)
            {
                PinataClassData classData = pinataData.Classes[i];

                writer.WriteLine("\tpublic class {0}Rectangles", classData.Prefix);
                writer.WriteLine("\t{");
    
                writer.WriteLine("\t\tprivate Rectangle[] rectangles;"); 
                writer.WriteLine();
                writer.WriteLine("\t\tpublic {0}Rectangles(Rectangle[] rectangles)", classData.Prefix);
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tthis.rectangles = rectangles;");
                writer.WriteLine("\t\t}");
                writer.WriteLine();

                for (int j = 0; j < classData.Pinboard.RectInfos.Count + 1; j++)
                {
                    RectangleInfo rectInfo;
                    
                    if (j == 0)
                        rectInfo = classData.Pinboard.ScreenRectInfo;
                    else
                        rectInfo = classData.Pinboard.RectInfos[j - 1];

                    writer.WriteLine("\t\tpublic Rectangle {0} {{ get {{ return rectangles[{1}]; }} }}",
                        rectInfo.Name, 
                        j);
                }

                writer.WriteLine("\t}");
                writer.WriteLine();
            }

            writer.WriteLine("}");
        }

        #region IProcessCommandLine Members

        public bool ProcessCommandLine(string[] args)
        {
            this.runningFromCommandLine = true;

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
