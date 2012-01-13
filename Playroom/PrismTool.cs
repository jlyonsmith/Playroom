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

        [CommandLineArgument("force", ShortName = "f", Description = "Force a full update of all PNG's instead of an incremental one", ValueHint = "<bool>")]
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

            if (!File.Exists(this.ConvertExe))
            {
                Output.Error("Convert tool not found at '{0}'", this.ConvertExe);
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

            if (prismData == null)
                return;

            foreach (var prismPinboard in prismData.Pinboards)
            {
                prismPinboard.FileName = prismPinboard.FileName.MakeFullPath();

                foreach (var mapping in prismPinboard.Mappings)
                {
                    mapping.PngFileName = mapping.PngFileName.MakeFullPath(this.PngDir);
                    
                    if (mapping.SvgFileName != null)
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
                    DateTime svgFileWriteTime = (mapping.SvgFileName != null ? File.GetLastWriteTime(mapping.SvgFileName) : DateTime.MinValue);
                    DateTime pngFileWriteTime = File.GetLastWriteTime(mapping.PngFileName);
                    DateTime pinboardFileWriteTime = File.GetLastWriteTime(prismPinboard.FileName);

                    if (Force || // We are being forced to update
                        svgFileWriteTime > pngFileWriteTime || // The .SVG is newer than the .PNG
                        prismFileWriteTime > pngFileWriteTime || // The .PRISM is newer than the .PNG
                        pinboardFileWriteTime > pngFileWriteTime) // The .PINBOARD is newer than the .PNG
                    {
                        RectangleInfo rectInfo;

                        if (mapping.RectangleName == "Screen")
                            rectInfo = prismPinboard.Pinboard.ScreenRectInfo;
                        else
                        {
                            rectInfo = prismPinboard.Pinboard.RectInfos.Find(r => r.Name == mapping.RectangleName);

                            if (rectInfo == null)
                            {
                                Output.Error("Rectangle '{0}' does not exist in pinboard '{1}'", mapping.RectangleName, prismPinboard.FileName);
                                return;
                            }
                        }

                        ParsedPath pngFileName = 
                            (Pad ? 
                                new ParsedPath(mapping.PngFileName.VolumeDirectoryAndFile + "_Temp" + 
                                    mapping.PngFileName.Extension, PathType.File) : 
                                mapping.PngFileName);

                        if (mapping.SvgFileName == null)
                        {
                            DrawPng(pngFileName, rectInfo);
                        }
                        else
                        {
                            if (!ConvertSvgToPng(mapping.SvgFileName, pngFileName, rectInfo.Width, rectInfo.Height, false))
                                return;
                        }

                        if (Pad)
                        {
                            PadPng(pngFileName, mapping.PngFileName);
                            File.Delete(pngFileName);
                        }
                    }
                }
            }
        }

        private bool ConvertSvgToPng(string svgFile, string pngFile, int width, int height, bool keepAspectRatio)
        {
            Output.Message(MessageImportance.Normal, "'{0}' -> '{1}'", svgFile, pngFile);

            string output;
            string command = string.Format("\"{0}\" \"{1}\" -w {2} -h {3} {4} -o \"{5}\"", 
                this.ConvertExe, // 0
                svgFile, // 1
                width.ToString(), // 2
                height.ToString(), // 3
                keepAspectRatio ? "-a" : "", // 4
                pngFile // 5
                );

            int ret = Command.Run(command, out output);

            if (ret != 0)
            {
                Output.Error("Error performing conversion: {0}", output);
                return false;
            }

            return true;
        }

        private void PadPng(string originalPngFileName, string newPngFileName)
        {
            using (Bitmap originalImage = new Bitmap(originalPngFileName))
            {
                using (Bitmap image = new Bitmap(RoundUpToPowerOf2(originalImage.Width), RoundUpToPowerOf2(originalImage.Height)))
                {
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        g.DrawImage(originalImage, new Point(0, 0));
                    }

                    SavePng(image, newPngFileName);
                }
            }
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

        private void DrawPng(string pngFileName, RectangleInfo rectInfo)
        {
            using (Bitmap image = new Bitmap(rectInfo.Width, rectInfo.Height))
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    DrawRectangleInfo(g, rectInfo.Width, rectInfo.Height, rectInfo.Color, rectInfo.Name);
                }

                SavePng(image, pngFileName);
            }
        }

        private void DrawRectangleInfo(Graphics g, int width, int height, Color color, string name)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, 0, 0, width, height);
            }

            float penWidth = 1.0f;
            using (Pen blackPen = new Pen(Color.FromArgb(color.A, Color.Black), penWidth))
            {
                Rectangle rect = new Rectangle(0, 0, width, height);
                
                DrawExactRectangle(g, blackPen, ref rect);

                int margin = 5;
                RectangleF textRect = new Rectangle(
                    margin, margin,
                    Math.Max(width - 2 * margin, 0), Math.Max(height - 2 * margin, 0));

                if (!textRect.IsEmpty)
                {
                    using (StringFormat format = new StringFormat())
                    {
                        g.DrawString(name, SystemFonts.IconTitleFont, Brushes.Black, textRect);
                    }
                }
            }
        }

        private void DrawExactRectangle(Graphics g, Pen pen, ref Rectangle rect)
        {
            float shrinkAmount = pen.Width / 2;

            g.DrawRectangle(pen,
                rect.X + shrinkAmount,
                rect.Y + shrinkAmount,
                rect.Width - pen.Width,
                rect.Height - pen.Width);
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
                Output.Error(e.Message);
                return false;
            }

            return true;
        }

        #endregion
    }
}
