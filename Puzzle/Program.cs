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

    [CommandLineDescription("Puzzle PNG generation tool")]
    [CommandLineTitle("Puzzle Tool")]
    public class PaintTool : IProcessCommandLine
    {
        [DefaultCommandLineArgument("default", Description = "Images data file", ValueHint = "<images-file>")]
        public ParsedPath ImagesFile { get; set; }

        [CommandLineArgument("outdir", ShortName = "d", Description = "Output directory", ValueHint = "<out-dir>")]
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

        public PaintTool(IOutputter outputter)
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

            if (String.IsNullOrEmpty(ImagesFile))
            {
                Output.Error("An images file must be specified");
                return;
            }

            Output.Message(MessageImportance.Normal, "Reading paintbrush file '{0}'", this.ImagesFile);

            PuzzleData data = ReadPuzzleData(this.ImagesFile);

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
