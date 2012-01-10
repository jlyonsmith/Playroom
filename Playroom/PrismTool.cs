using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.IO;
using System.Xml;

namespace Playroom
{
    [CommandLineDescription("Generates PNG files from SVG files using Inkscape.")]
    [CommandLineTitle("Prism Tool")]
    public class PrismTool : ITool, IProcessCommandLine
    {
        private bool runningFromCommandLine = false;

        [DefaultCommandLineArgument("default", Description = "Prism data file", ValueHint = "<prism-file>")]
        public ParsedPath PrismFile { get; set; }

        [CommandLineArgument("pngdir", ShortName = "pd", Description = "Output root directory for PNG files", ValueHint = "<out-dir>",
            Initializer = typeof(PrismTool), MethodName = "ParseOutputDir")]
        public ParsedPath PngDir { get; set; }

        [CommandLineArgument("svgdir", ShortName = "sd", Description = "Input root directory for SVG files", ValueHint = "<in-dir>",
            Initializer = typeof(PrismTool), MethodName = "ParseOutputDir")]
        public ParsedPath SvgDir { get; set; }

        [CommandLineArgument("convert", ShortName = "c", Description = "Path to ImageMagick convert executable", ValueHint = "<convert-exe>")]
        public ParsedPath ConvertExe { get; set; }

        [CommandLineArgument("force", Description = "Force a full update of all PNG's instead of an incremental one", ValueHint = "<bool>")]
        public bool Force { get; set; }

        [CommandLineArgument("pad", ShortName = "p", Description = "Pad the width and height of all images to a power of two", ValueHint = "<bool>")]
        public bool Pad { get; set; }

        [CommandLineArgument("help", Description = "Displays this help", ShortName = "?")]
        public bool ShowHelp { get; set; }

        [CommandLineArgument("nologo", Description = "Suppress logo banner")]
        public bool NoLogo { get; set; }

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

        public PrismTool(IOutputter outputter)
        {
            this.Output = new OutputHelper(outputter);
        }

        public static ParsedPath ParseOutputDir(string arg)
        {
            return new ParsedPath(arg, PathType.Directory);
        }

        public void Execute()
        {
            if (!NoLogo)
            {
                Console.WriteLine(Parser.LogoBanner);
            }

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

            if (String.IsNullOrEmpty(PrismFile))
            {
                Output.Error("A prism file must be specified");
                return;
            }

            this.SvgDir = this.SvgDir.MakeFullPath();
            this.PngDir = this.PngDir.MakeFullPath();
            this.ConvertExe = this.ConvertExe.MakeFullPath();
            this.PrismFile = this.PrismFile.MakeFullPath();

            if (!File.Exists(this.PrismFile))
            {
                Output.Error("Prism file '{0}' not found", this.PrismFile);
                return;
            }

            if (!Directory.Exists(this.SvgDir))
            {
                Directory.CreateDirectory(this.SvgDir);
            }

            if (!Directory.Exists(this.PngDir))
            {
                Directory.CreateDirectory(this.PngDir);
            }

            Output.Message(MessageImportance.Low, "Reading prism file '{0}'", this.PrismFile);

            PrismData prismData = ReadPrismData(this.PrismFile);

            foreach (var prismPinboard in prismData.Pinboards)
            {
                prismPinboard.FileName = prismPinboard.FileName.MakeFullPath();

                foreach (var mapping in prismPinboard.Mappings)
                {
                    mapping.PngFileName = mapping.PngFileName.MakeFullPath(this.PngDir);
                    mapping.SvgFileName = mapping.SvgFileName.MakeFullPath(this.SvgDir);
                }
            }

            DateTime prismFileWriteTime = File.GetLastWriteTime(this.PrismFile);

            foreach (var prismPinboard in prismData.Pinboards)
            {
                Output.Message(MessageImportance.Low, "Reading pinboard file '{0}'", prismPinboard.FileName);

                prismPinboard.Pinboard = ReadPinboardData(prismPinboard.FileName);

                if (prismPinboard.Pinboard == null)
                    return;
            }

            foreach (var prismPinboard in prismData.Pinboards)
            {
                foreach (var mapping in prismPinboard.Mappings)
                {
                    DateTime svgFileWriteTime = File.GetLastWriteTime(mapping.SvgFileName);
                    DateTime pngFileWriteTime = File.GetLastWriteTime(mapping.PngFileName);

                    if (Force || svgFileWriteTime > pngFileWriteTime || prismFileWriteTime > pngFileWriteTime)
                    {
                        RectangleInfo rectInfo = prismPinboard.Pinboard.RectInfos.Find(r => r.Name == mapping.RectangleName);

                        if (rectInfo == null)
                        {
                            Output.Error("Rectangle '{0}' does not exist in pinboard '{1}'", mapping.RectangleName, prismPinboard.FileName);
                            return;
                        }

                        if (!ConvertSvgToPng(mapping.SvgFileName, mapping.PngFileName, rectInfo.Width, rectInfo.Height, false))
                            return;
                    }
                }
            }
        }

        private bool ConvertSvgToPng(string svgFile, string pngFile, int width, int height, bool keepAspectRatio)
        {
            Output.Message(MessageImportance.Normal, "'{0}' -> '{1}'", svgFile, pngFile);

            int extentWidth;
            int extentHeight;

            if (Pad)
            {
                extentWidth = RoundUpToPowerOf2(width);
                extentHeight = RoundUpToPowerOf2(height);
            }
            else
            {
                extentWidth = width;
                extentHeight = height;
            }

            string output;
            string command = string.Format("\"{0}\" -background none \"{1}\" -resize {2}x{3}{4} -extent {5}x{6} \"{7}\"", 
                this.ConvertExe, // 0
                svgFile, // 1
                width.ToString(), // 2
                height.ToString(), // 3
                keepAspectRatio ? "" : "!", // 4
                extentWidth.ToString(), // 5
                extentHeight.ToString(), // 6
                pngFile // 7
                );

            int ret = Command.Run(command, out output);

            if (ret != 0)
            {
                Output.Error("Error performing conversion: {0}", output);
                return false;
            }

            return true;
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

        private PrismData ReadPrismData(string fileName)
        {
            PrismData data = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(fileName))
                {
                    data = PrismDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception ex)
            {
                Output.Error("Unable to read Pinboard file '{0}': {1}", fileName, ex.Message);
                return null;
            }

            return data;
        }

        private int RoundUpToPowerOf2(int n)
        {
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n + 1;
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
