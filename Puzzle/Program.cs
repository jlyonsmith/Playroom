using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using ToolBelt;
using Playroom;

namespace Puzzle
{
    class Program
    {
        static int Main(string[] args)
        {
            PaintTool tool = new PaintTool(new ConsoleOutputter());

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

    [CommandLineDescription("Generates PNG files from SVG files using Inkscape.")]
    [CommandLineTitle("Puzzle Tool")]
    public class PaintTool : IProcessCommandLine
    {
        [DefaultCommandLineArgument("default", Description = "Puzzle data file", ValueHint = "<puzzle-file>")]
        public ParsedPath PuzzleFile { get; set; }

        [CommandLineArgument("outdir", ShortName = "o", Description = "Output directory", ValueHint = "<out-dir>", 
            Initializer = typeof(PaintTool), MethodName = "ParseOutputDir")]
        public ParsedPath OutputDir { get; set; }

        [CommandLineArgument("inkscape", ShortName = "i", Description = "Path to Inkscape executable", ValueHint = "<inkscape-exe>")]
        public ParsedPath InkscapeFile { get; set; }

        [CommandLineArgument("full", ShortName = "f", Description = "Do a full update instead of an incremental one", ValueHint = "<bool>")]
        public bool FullUpdate { get; set; }

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

        public PaintTool(IOutputter outputter)
        {
            this.Output = new OutputHelper(outputter);
        }

        private static ParsedPath ParseOutputDir(string arg)
        {
            return new ParsedPath(arg, PathType.Directory);
        }

        public void Execute()
        {
            Console.WriteLine(Parser.LogoBanner);

            if (ShowHelp)
            {
                Console.WriteLine(Parser.Usage);
                return;
            }

            if (String.IsNullOrEmpty(PuzzleFile))
            {
                Output.Error("An puzzle file must be specified");
                return;
            }

            Output.Message(MessageImportance.Normal, "Reading puzzle file '{0}'", this.PuzzleFile);

            PuzzleData data = ReadPuzzleData(this.PuzzleFile);

            foreach (var pair in data.PuzzlePinboards)
            {
                PuzzlePinboard mappingData = pair.Value;
                
                Output.Message(MessageImportance.Low, "Reading pinboard file '{0}'", pair.Key);

                PinboardData pinData = ReadPinboardData(pair.Key);

                if (pinData == null)
                {
                    return;
                }

                pair.Value.Pinboard = pinData;
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

        private PuzzleData ReadPuzzleData(string fileName)
        {
            PuzzleData data = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(fileName))
                {
                    data = PuzzleDataReaderV1.ReadXml(reader);
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
