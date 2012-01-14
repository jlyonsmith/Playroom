﻿using System;
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
                this.PngFile = pngFile;
                this.TargetRectangle = targetRectangle;
            }

            public ParsedPath PngFile { get; set; }
            public Rectangle TargetRectangle { get; set; }
        }

        private bool runningFromCommandLine = false;

        [DefaultCommandLineArgument("default", Description = "Prism data file", ValueHint = "<prism-file>")]
        public ParsedPath PrismFile { get; set; }

        [CommandLineArgument("pngdir", ShortName = "pd", Description = "Output root directory for PNG files", ValueHint = "<out-dir>",
            Initializer = typeof(PrismTool), MethodName = "ParseOutputDir")]
        public ParsedPath PngDir { get; set; }

        [CommandLineArgument("svgdir", ShortName = "sd", Description = "Input root directory for SVG files", ValueHint = "<in-dir>",
            Initializer = typeof(PrismTool), MethodName = "ParseOutputDir")]
        public ParsedPath SvgDir { get; set; }

        [CommandLineArgument("rsvg", ShortName = "r", Description = "Path to rsvg-convert.exe binary", ValueHint = "<rsvg-convert-exe>")]
        public ParsedPath RsvgConvertExe { get; set; }

        [CommandLineArgument("inkscape", ShortName = "i", Description = "Path to inkscape.exe binary", ValueHint = "<inkscape-exe>")]
        public ParsedPath InkscapeExe { get; set; }

        [CommandLineArgument("force", ShortName = "f", Description = "Force a full update of all PNG's instead of an incremental one", ValueHint = "<bool>")]
        public bool Force { get; set; }

        [CommandLineArgument("extend", ShortName = "x", Description = "Extend the width and height of all images to a power of two", ValueHint = "<bool>")]
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

            this.SvgDir = this.SvgDir.MakeFullPath();
            this.PngDir = this.PngDir.MakeFullPath();
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

            if (this.InkscapeExe != null && !File.Exists(this.InkscapeExe))
            {
                Output.Error("inkscape.exe tool not found at '{0}'", this.InkscapeExe);
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

                foreach (var prismCompound in prismPinboard.Compounds)
                {
                    prismCompound.PngFileName = prismCompound.PngFileName.MakeFullPath(this.PngDir);
                }

                foreach (var mapping in prismPinboard.Mappings)
                {
                    mapping.PngFileName = mapping.PngFileName.MakeFullPath(this.PngDir);

                    if (mapping.SvgFileName != null)
                    {
                        mapping.SvgFileName = mapping.SvgFileName.MakeFullPath(this.SvgDir);

                        if (!File.Exists(mapping.SvgFileName))
                        {
                            Output.Error(PrismFile, mapping.LineNumber, 0, "File '{0}' does not exist", mapping.SvgFileName);
                            return;
                        }
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
                        PrismCompound prismCompound = prismPinboard.Compounds.Find(c => c.PngFileName == prismMapping.PngFileName);

                        if (prismCompound == null)
                        {
                            Output.Error(this.PrismFile, prismMapping.LineNumber, 0, 
                                "Cannot find a rectangle name for compound PNG file '{0}'", prismMapping.PngFileName);
                            return;
                        }

                        if (prismCompound.Mappings == null)
                        {
                            prismCompound.Mappings = new PrismMapping[prismCompound.ColumnCount, prismCompound.RowCount];
                        }

                        if (prismCompound.NextRow == prismCompound.RowCount)
                        {
                            Output.Error(this.PrismFile, prismCompound.LineNumber, 0, 
                                "Too many mappings specified for compound image '{0}'", prismCompound.PngFileName);
                            return;
                        }

                        prismMapping.RectangleName = prismCompound.RectangleName;
                        prismMapping.Compound = prismCompound;
                        prismMapping.PngFileName = new ParsedPath(
                            prismMapping.PngFileName.VolumeDirectoryAndFile + 
                            "_" + prismCompound.NextColumn + "_" + prismCompound.NextRow + 
                            prismMapping.PngFileName.Extension, PathType.File);

                        DateTime svgFileWriteTime =
                            (prismMapping.SvgFileName != null ?
                            File.GetLastWriteTime(prismMapping.SvgFileName) :
                            DateTime.MinValue);

                        if (svgFileWriteTime > prismCompound.NewestSvgFileWriteTime)
                            prismCompound.NewestSvgFileWriteTime = svgFileWriteTime;

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
                            "Compound image '{0}' does not have enough mappings", prismCompound.PngFileName);
                    }
                }

                foreach (var prismMapping in prismPinboard.Mappings)
                {
                    DateTime svgFileWriteTime = 
                        (prismMapping.Compound != null ? 
                        prismMapping.Compound.NewestSvgFileWriteTime : 
                        prismMapping.SvgFileName != null ? 
                        File.GetLastWriteTime(prismMapping.SvgFileName) : 
                        DateTime.MinValue);
                    DateTime pngFileWriteTime = 
                        (prismMapping.Compound != null ?
                        File.GetLastWriteTime(prismMapping.Compound.PngFileName) :
                        File.GetLastWriteTime(prismMapping.PngFileName));
                    DateTime pinboardFileWriteTime = File.GetLastWriteTime(prismPinboard.FileName);

                    if (Force || // We are being forced to update
                        svgFileWriteTime > pngFileWriteTime || // The .SVG is newer than the .PNG
                        prismFileWriteTime > pngFileWriteTime || // The .PRISM is newer than the .PNG
                        pinboardFileWriteTime > pngFileWriteTime) // The .PINBOARD is newer than the .PNG
                    {
                        if (prismMapping.SvgFileName == null)
                        {
                            DrawPng(prismMapping.PngFileName, prismMapping.RectangleInfo);
                        }
                        else
                        {
                            if (!ConvertSvgToPng(
                                prismMapping.SvgFileName, prismMapping.PngFileName,
                                prismMapping.RectangleInfo.Width, prismMapping.RectangleInfo.Height, false))
                            {
                                return;
                            }
                        }

                        if (prismMapping.Compound != null)
                            prismMapping.Compound.PngNeedsCompounding = true;

                        if (Extend)
                        {
                            if (prismMapping.Compound != null)
                                extendPngFileNames.Add(prismMapping.Compound.PngFileName);
                            else
                                extendPngFileNames.Add(prismMapping.PngFileName);
                        }
                    }
                }

                foreach (var prismCompound in prismPinboard.Compounds)
                {
                    if (!prismCompound.PngNeedsCompounding)
                        continue;

                    List<ImagePlacement> placements = new List<ImagePlacement>();

                    for (int r = 0; r < prismCompound.RowCount; r++)
                    {
                        for (int c = 0; c < prismCompound.ColumnCount; c++)
                        {
                            PrismMapping prismMapping = prismCompound.Mappings[c, r];
                            RectangleInfo rectInfo = prismMapping.RectangleInfo;

                            placements.Add(new ImagePlacement(prismMapping.PngFileName, 
                                new Rectangle(c * rectInfo.Width, r * rectInfo.Height, rectInfo.Width, rectInfo.Height)));
                        }
                    }

                    if (placements.Count > 0)
                        CombinePngs(placements, prismCompound.PngFileName);

                    foreach (var placement in placements)
                    {
                        File.Delete(placement.PngFile);
                    }
               }
            }

            foreach (var pngFileName in extendPngFileNames)
            {
                ParsedPath tempPngFileName = new ParsedPath(
                    pngFileName.VolumeDirectoryAndFile + "_Temp" + pngFileName.Extension, PathType.File);
                ExtendPng(pngFileName, tempPngFileName);
                File.Delete(pngFileName);
                File.Move(tempPngFileName, pngFileName);
            }
        }

        private void CombinePngs(List<ImagePlacement> placements, ParsedPath pngFileName)
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

                sb.AppendFormat(placement.PngFile + Environment.NewLine);
            }

            using (Bitmap combinedImage = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(combinedImage))
                {
                    foreach (var placement in placements)
                    {
                        using (Bitmap image = new Bitmap(placement.PngFile))
                        {
                            g.DrawImage(image, new Point(placement.TargetRectangle.X, placement.TargetRectangle.Y));
                        }
                    }
                }

                SavePng(combinedImage, pngFileName);
            }

            sb.AppendFormat("-> {0}", pngFileName);
            Output.Message(sb.ToString());
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
                Output.Error("Error performing conversion: {0}", output);
                return false;
            }

            Output.Message(MessageImportance.Normal, "'{0}' -> '{1}'", svgFile, pngFile);
            return true;
        }

        private void ExtendPng(string originalPngFileName, string newPngFileName)
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
