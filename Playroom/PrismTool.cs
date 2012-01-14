using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;

namespace Playroom
{
    [CommandLineDescription("Generates PNG files from SVG files using Inkscape.")]
    [CommandLineTitle("Prism Tool")]
    public class PrismTool : ITool, IProcessCommandLine
    {
        private class ImagePlacement
        {
            public ImagePlacement(ParsedPath pngFile, Rectangle targetRectangle)
            {
                this.ImageFile = pngFile;
                this.TargetRectangle = targetRectangle;
            }

            public ParsedPath ImageFile { get; set; }
            public Rectangle TargetRectangle { get; set; }
        }

        private bool runningFromCommandLine = false;

        [DefaultCommandLineArgument("default", Description = "Prism data file", ValueHint = "<prism-file>")]
        public ParsedPath PrismFile { get; set; }

        [CommandLineArgument("outdir", ShortName = "od", Description = "Output file root directory", ValueHint = "<out-dir>",
            Initializer = typeof(PrismTool), MethodName = "ParseOutputDir")]
        public ParsedPath OutputDirectory { get; set; }

        [CommandLineArgument("indir", ShortName = "id", Description = "Input file root directory", ValueHint = "<in-dir>",
            Initializer = typeof(PrismTool), MethodName = "ParseOutputDir")]
        public ParsedPath InputDirectory { get; set; }

        [CommandLineArgument("rsvg", ShortName = "r", Description = "Path to rsvg-convert.exe binary", ValueHint = "<rsvg-convert-exe>")]
        public ParsedPath RsvgConvertExe { get; set; }

        [CommandLineArgument("force", ShortName = "f", Description = "Force a full update of all PNG's instead of an incremental one", ValueHint = "<bool>")]
        public bool Force { get; set; }

        [CommandLineArgument("extend", ShortName = "x", Description = "Extend the width and height of all output images to a power of two", ValueHint = "<bool>")]
        public bool Extend { get; set; }

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

            this.InputDirectory = this.InputDirectory.MakeFullPath();
            this.OutputDirectory = this.OutputDirectory.MakeFullPath();
            this.RsvgConvertExe = this.RsvgConvertExe.MakeFullPath();
            this.PrismFile = this.PrismFile.MakeFullPath();

            if (!File.Exists(this.PrismFile))
            {
                Output.Error("Prism file '{0}' not found", this.PrismFile);
                return;
            }

            if (!File.Exists(this.RsvgConvertExe))
            {
                Output.Error("rsvg-convert.exe tool not found at '{0}'", this.RsvgConvertExe);
                return;
            }

            if (!Directory.Exists(this.InputDirectory))
            {
                Directory.CreateDirectory(this.InputDirectory);
            }

            if (!Directory.Exists(this.OutputDirectory))
            {
                Directory.CreateDirectory(this.OutputDirectory);
            }

            Output.Message(MessageImportance.Low, "Reading prism file '{0}'", this.PrismFile);

            PrismData prismData = ReadPrismData(this.PrismFile);

            if (prismData == null)
                return;

            foreach (var prismPinboard in prismData.Pinboards)
            {
                prismPinboard.FileName = prismPinboard.FileName.MakeFullPath();

                foreach (var prismCompound in prismPinboard.Compounds)
                {
                    prismCompound.OutputFileName = prismCompound.OutputFileName.MakeFullPath(this.OutputDirectory);
                }

                foreach (var mapping in prismPinboard.Mappings)
                {
                    mapping.OutputFileName = mapping.OutputFileName.MakeFullPath(this.OutputDirectory);
                    mapping.InputFileName = mapping.InputFileName.MakeFullPath(this.InputDirectory);

                    if (!File.Exists(mapping.InputFileName))
                    {
                        Output.Error(PrismFile, mapping.LineNumber, 0, "File '{0}' does not exist", mapping.InputFileName);
                        return;
                    }
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

            List<ParsedPath> extendPngFileNames = new List<ParsedPath>();

            foreach (var prismPinboard in prismData.Pinboards)
            {
                foreach (var prismMapping in prismPinboard.Mappings)
                {
                    if (prismMapping.RectangleName == null)
                    {
                        PrismCompound prismCompound = prismPinboard.Compounds.Find(c => c.OutputFileName == prismMapping.OutputFileName);

                        if (prismCompound == null)
                        {
                            Output.Error(this.PrismFile, prismMapping.LineNumber, 0, 
                                "Cannot find a rectangle name for compounded output file '{0}'", prismMapping.OutputFileName);
                            return;
                        }

                        if (prismCompound.Mappings == null)
                        {
                            prismCompound.Mappings = new PrismMapping[prismCompound.ColumnCount, prismCompound.RowCount];
                        }

                        if (prismCompound.NextRow == prismCompound.RowCount)
                        {
                            Output.Error(this.PrismFile, prismCompound.LineNumber, 0, 
                                "Too many mappings specified for compound image '{0}'", prismCompound.OutputFileName);
                            return;
                        }

                        prismMapping.RectangleName = prismCompound.RectangleName;
                        prismMapping.Compound = prismCompound;
                        prismMapping.OutputFileName = new ParsedPath(
                            prismMapping.OutputFileName.VolumeDirectoryAndFile + 
                            "_" + prismCompound.NextColumn + "_" + prismCompound.NextRow + 
                            prismMapping.OutputFileName.Extension, PathType.File);

                        DateTime inputFileWriteTime = File.GetLastWriteTime(prismMapping.InputFileName);

                        if (inputFileWriteTime > prismCompound.NewestInputFileWriteTime)
                            prismCompound.NewestInputFileWriteTime = inputFileWriteTime;

                        prismCompound.Mappings[prismCompound.NextColumn, prismCompound.NextRow] = prismMapping;

                        prismCompound.NextColumn++;

                        if (prismCompound.NextColumn == prismCompound.ColumnCount)
                        {
                            prismCompound.NextRow++;
                            prismCompound.NextColumn = 0;
                        }
                    }

                    if (prismMapping.RectangleName == "Screen")
                        prismMapping.RectangleInfo = prismPinboard.Pinboard.ScreenRectInfo;
                    else
                    {
                        prismMapping.RectangleInfo = prismPinboard.Pinboard.RectInfos.Find(r => r.Name == prismMapping.RectangleName);

                        if (prismMapping.RectangleInfo == null)
                        {
                            Output.Error("Rectangle '{0}' does not exist in pinboard '{1}'", 
                                prismMapping.RectangleName, prismPinboard.FileName);
                            return;
                        }
                    }
                }

                foreach (var prismCompound in prismPinboard.Compounds)
                {
                    if (prismCompound.NextRow != prismCompound.RowCount &&
                        prismCompound.NextColumn != prismCompound.ColumnCount)
                    {
                        Output.Error(this.PrismFile, prismCompound.LineNumber, 0, 
                            "Compound image '{0}' does not have enough mappings", prismCompound.OutputFileName);
                    }
                }

                foreach (var prismMapping in prismPinboard.Mappings)
                {
                    DateTime inputFileWriteTime = 
                        (prismMapping.Compound != null ? 
                            prismMapping.Compound.NewestInputFileWriteTime : 
                            File.GetLastWriteTime(prismMapping.InputFileName));
                    DateTime outputFileWriteTime = 
                        (prismMapping.Compound != null ?
                        File.GetLastWriteTime(prismMapping.Compound.OutputFileName) :
                        File.GetLastWriteTime(prismMapping.OutputFileName));
                    DateTime pinboardFileWriteTime = File.GetLastWriteTime(prismPinboard.FileName);

                    if (Force || // We are being forced to update
                        inputFileWriteTime > outputFileWriteTime || // The .SVG is newer than the .PNG
                        prismFileWriteTime > outputFileWriteTime || // The .PRISM is newer than the .PNG
                        pinboardFileWriteTime > outputFileWriteTime) // The .PINBOARD is newer than the .PNG
                    {
                        if (String.Compare(prismMapping.InputFileName.Extension, ".png", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            if (!ScaleImage(prismMapping.InputFileName, prismMapping.RectangleInfo.Size, prismMapping.OutputFileName))
                                return;
                        }
                        else if (String.Compare(prismMapping.InputFileName.Extension, ".svg", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            if (!ConvertSvgToPng(
                                prismMapping.InputFileName, prismMapping.OutputFileName,
                                prismMapping.RectangleInfo.Width, prismMapping.RectangleInfo.Height, false))
                            {
                                return;
                            }
                        }
                        else
                        {
                            Output.Error(PrismFile, prismMapping.LineNumber, 0, 
                                "Unrecognized input file extension '{0}'", prismMapping.InputFileName.Extension);
                            return;
                        }

                        if (prismMapping.Compound != null)
                            prismMapping.Compound.OutputFileNeedsCompounding = true;

                        if (Extend)
                        {
                            if (prismMapping.Compound != null)
                                extendPngFileNames.Add(prismMapping.Compound.OutputFileName);
                            else
                                extendPngFileNames.Add(prismMapping.OutputFileName);
                        }
                    }
                }

                foreach (var prismCompound in prismPinboard.Compounds)
                {
                    if (!prismCompound.OutputFileNeedsCompounding)
                        continue;

                    List<ImagePlacement> placements = new List<ImagePlacement>();

                    for (int r = 0; r < prismCompound.RowCount; r++)
                    {
                        for (int c = 0; c < prismCompound.ColumnCount; c++)
                        {
                            PrismMapping prismMapping = prismCompound.Mappings[c, r];
                            RectangleInfo rectInfo = prismMapping.RectangleInfo;

                            placements.Add(new ImagePlacement(prismMapping.OutputFileName, 
                                new Rectangle(c * rectInfo.Width, r * rectInfo.Height, rectInfo.Width, rectInfo.Height)));
                        }
                    }

                    if (placements.Count > 0)
                    {
                        if (!CombineImages(placements, prismCompound.OutputFileName))
                            return;
                    }

                    foreach (var placement in placements)
                    {
                        File.Delete(placement.ImageFile);
                    }
               }
            }

            foreach (var outputFileName in extendPngFileNames)
            {
                ParsedPath tempFileName = new ParsedPath(
                    outputFileName.VolumeDirectoryAndFile + "_Temp" + outputFileName.Extension, PathType.File);
                
                if (!ExtendImage(outputFileName, tempFileName))
                {
                    return;
                }

                try
                {
                    File.Delete(outputFileName);
                }
                catch (Exception e)
                {
                    Output.Error("Unable to delete file '{0}': {2}", outputFileName, e.Message);
                }

                try
                {
                    File.Move(tempFileName, outputFileName);
                }
                catch (Exception e)
                {
                    Output.Error("Unable to move '{0}' to '{1}': {2}", tempFileName, outputFileName, e.Message);
                }
            }
        }

        private bool CombineImages(List<ImagePlacement> placements, ParsedPath imageFileName)
        {
            try
            {
                int width = 0;
                int height = 0;
                StringBuilder sb = new StringBuilder();

                foreach (var placement in placements)
                {
                    if (placement.TargetRectangle.Right > width)
                        width = placement.TargetRectangle.Right;

                    if (placement.TargetRectangle.Bottom > height)
                        height = placement.TargetRectangle.Bottom;

                    sb.AppendFormat(placement.ImageFile + Environment.NewLine);
                }

                using (Bitmap combinedImage = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(combinedImage))
                    {
                        foreach (var placement in placements)
                        {
                            using (Bitmap image = new Bitmap(placement.ImageFile))
                            {
                                g.DrawImage(image, new Point(placement.TargetRectangle.X, placement.TargetRectangle.Y));
                            }
                        }
                    }

                    SavePng(combinedImage, imageFileName);
                }

                sb.AppendFormat("-> {0}", imageFileName);
                Output.Message(sb.ToString());
            }
            catch (Exception e)
            {
                Output.Error("Unable to write compound output file '{0}': {1}", imageFileName, e.Message);
                return false;
            }

            return true;
        }

        private bool ConvertSvgToPng(string svgFile, string pngFile, int width, int height, bool keepAspectRatio)
        {
            string output;
            string command = string.Format("\"{0}\" \"{1}\" -w {2} -h {3} {4} -o \"{5}\"", 
                this.RsvgConvertExe, // 0
                svgFile, // 1
                width.ToString(), // 2
                height.ToString(), // 3
                keepAspectRatio ? "-a" : "", // 4
                pngFile // 5
                );

            int ret = Command.Run(command, out output);

            if (ret != 0)
            {
                Output.Error("Error performing conversion of '{0}' into '{1}':  {2}", 
                    svgFile, pngFile, output);
                return false;
            }

            Output.Message(MessageImportance.Normal, "'{0}' -> '{1}'", svgFile, pngFile);
            return true;
        }

        private bool ExtendImage(string originalFileName, string newFileName)
        {
            try
            {
                using (Bitmap originalImage = new Bitmap(originalFileName))
                {
                    using (Bitmap image = new Bitmap(RoundUpToPowerOf2(originalImage.Width), RoundUpToPowerOf2(originalImage.Height)))
                    {
                        using (Graphics g = Graphics.FromImage(image))
                        {
                            g.DrawImage(originalImage, new Point(0, 0));
                        }

                        SavePng(image, newFileName);
                    }
                }
            }
            catch (Exception e)
            {
                Output.Error("Unable to extend file '{0}' into file '{1}': {2}", originalFileName, newFileName, e.Message);
                return false;
            }

            return true;
        }

        private bool ScaleImage(string originalFileName, Size newSize, string newFileName)
        {
            try
            {
                using (Bitmap originalImage = new Bitmap(originalFileName))
                {
                    using (Bitmap image = new Bitmap(originalImage, newSize))
                    {
                        using (Graphics g = Graphics.FromImage(image))
                        {
                            g.DrawImage(originalImage, new Point(0, 0));
                        }

                        SavePng(image, newFileName);
                    }
                }
            }
            catch (Exception e)
            {
                Output.Error("Unable to scale image file '{0}' to '{1}': {2}", originalFileName, newFileName, e.Message);
                return false;
            }

            return true;
        }

        private void SavePng(Image image, string pngFileName)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);

                using (Stream fileStream = new FileStream(pngFileName, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }
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
            catch (Exception e)
            {
                Output.Error("Unable to read file '{0}': {1}", fileName, e.Message);
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
            catch (Exception e)
            {
                Output.Error("Unable to read file '{0}': {1}", fileName, e.Message);
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
                Output.Error(e.Message);
                return false;
            }

            return true;
        }

        #endregion
    }
}
