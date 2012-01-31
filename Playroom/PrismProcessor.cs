using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using ToolBelt;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Xml;

namespace Playroom
{
    [ContentProcessor(DisplayName = "Prism Processor")]
    public class PrismProcessor : ContentProcessor<PrismData, TextureContent>
    {
        private class ImagePlacement
        {
            public ImagePlacement(ParsedPath pngFile, System.Drawing.Rectangle targetRectangle)
            {
                this.ImageFile = pngFile;
                this.TargetRectangle = targetRectangle;
            }

            public ParsedPath ImageFile { get; set; }
            public System.Drawing.Rectangle TargetRectangle { get; set; }
        }

        private static ParsedPath InkscapeCom { get; set; }
        private static ParsedPath RSvgConvertExe { get; set; }

        private ContentProcessorContext Context { get; set; }
        private ParsedPath PrismFile { get; set; }

        static PrismProcessor()
        {
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"svgfile\shell\Inkscape\command", false);

                if (key != null)
                {
                    string s = (string)key.GetValue("");

                    if (s != null && s.Length > 0)
                    {
                        if (s[0] == '"')
                            s = s.Substring(1, s.IndexOf('"', 1) - 1);

                        ParsedPath path  = new ParsedPath(s, PathType.File).SetExtension(".com");

                        if (File.Exists(path))
                            PrismProcessor.InkscapeCom = path;
                    }
                }

                key = Registry.ClassesRoot.OpenSubKey(@"svgfile\shell\Edit with GIMP\command", false);

                if (key != null)
                {
                    string s = (string)key.GetValue("");

                    if (s != null && s.Length > 0)
                    {
                        if (s[0] == '"')
                            s = s.Substring(1, s.IndexOf('"', 1) - 1);

                        ParsedPath path = new ParsedPath(s, PathType.File).SetFileAndExtension("rsvg-convert.exe");

                        if (File.Exists(path))
                            PrismProcessor.RSvgConvertExe = path;
                    }
                }
            }
            catch
            {
            }
        }

        public override TextureContent Process(PrismData prismData, ContentProcessorContext context)
        {
            ParsedPath intermediateDir = new ParsedPath(context.IntermediateDirectory, PathType.Directory);

            this.Context = context;
            this.PrismFile = prismData.PrismFile;

            // Make all .svg paths absolute and add dependencies on them
            for (int i = 0; i < prismData.SvgFiles.Count; i++)
            {
                List<ParsedPath> list = prismData.SvgFiles[i];

                for (int j = 0; j < list.Count; j++)
                {
                    list[j] = list[j].MakeFullPath(prismData.SvgDirectory == null ? prismData.PrismFile : prismData.SvgDirectory);

                    context.AddDependency(list[j]);
                }
            }

            // Grab the pinboard data and add a dependency
            prismData.Pinboard = ReadPinboardFile(prismData.PinboardFile);
            context.AddDependency(prismData.PinboardFile);

            ParsedPath tmpPath = new ParsedPath(context.IntermediateDirectory, PathType.Directory);

            List<ImagePlacement> placements = new List<ImagePlacement>();

            try
            {
                // Go through each SVG and output a temporary PNG file.  Create an ImagePlacement for each SVG/PNG processed
                for (int row = 0; row < prismData.SvgFiles.Count; row++)
                {
                    List<ParsedPath> pathList = prismData.SvgFiles[row];

                    for (int col = 0; col < pathList.Count; col++)
                    {
                        RectangleInfo rectInfo = prismData.Pinboard.GetRectangleInfoByName(prismData.RectangleName);

                        if (rectInfo == null)
                            throw new InvalidContentException(
                                String.Format("Rectangle '{0}' not found in pinboard '{1}'", prismData.RectangleName, prismData.PinboardFile),
                                new ContentIdentity(prismData.PrismFile));

                        ParsedPath tmpPngFile = tmpPath.SetFileAndExtension(String.Format("{0}_{1}_{2}.png", prismData.PngFile.File, row, col));

                        placements.Add(new ImagePlacement(tmpPngFile, 
                            new Rectangle(col * rectInfo.Width, row * rectInfo.Height, rectInfo.Width, rectInfo.Height)));

                        switch (prismData.Converter)
                        {
                            default:
                            case SvgToPngConverter.RSvg:
                                ConvertSvgToPngWithRSvg(pathList[col], tmpPngFile, rectInfo.Width, rectInfo.Height);
                                break;

                            case SvgToPngConverter.Inkscape:
                                ConvertSvgToPngWithInkscape(pathList[col], tmpPngFile, rectInfo.Width, rectInfo.Height);
                                break;
                        }
                    }
                }

                // Combine all the PNG files into the final PNG using the ImagePlacements and delete the temp files.
                CombineImages(placements, prismData.PngFile);
            }
            finally
            {
                foreach (var placement in placements)
                {
                    if (File.Exists(placement.ImageFile))
                        File.Delete(placement.ImageFile);
                }
            }

            OpaqueDataDictionary processorParams = new OpaqueDataDictionary();
            ExternalReference<TextureContent> exRef = new ExternalReference<TextureContent>(prismData.PngFile);
            TextureContent textureContent = context.BuildAndLoadAsset<TextureContent, TextureContent>(exRef, null, processorParams, null);

            return textureContent;
        }

        private PinboardData ReadPinboardFile(ParsedPath pinboardFile)
        {
            PinboardData data = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(pinboardFile))
                {
                    data = PinboardDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception e)
            {
                throw new InvalidContentException(String.Format("Unable to read pinboard file '{0}'", pinboardFile),
                    new ContentIdentity(this.PrismFile), e);
            }

            return data;
        }

        private bool CombineImages(List<ImagePlacement> placements, ParsedPath imageFileName)
        {
            Context.Logger.LogMessage("Combining images into {0}", imageFileName);

            try
            {
                int width = 0;
                int height = 0;

                foreach (var placement in placements)
                {
                    if (placement.TargetRectangle.Right > width)
                        width = placement.TargetRectangle.Right;

                    if (placement.TargetRectangle.Bottom > height)
                        height = placement.TargetRectangle.Bottom;
                }

                using (Bitmap combinedImage = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(combinedImage))
                    {
                        foreach (var placement in placements)
                        {
                            using (Bitmap image = new Bitmap(placement.ImageFile))
                            {
                                g.DrawImageUnscaled(image, placement.TargetRectangle);
                            }
                        }
                    }

                    SavePng(combinedImage, imageFileName);
                }
            }
            catch (Exception e)
            {
                throw new InvalidContentException(String.Format("Unable to combine images for image file '{0}'. {1}", imageFileName, e.Message), 
                    new ContentIdentity(imageFileName), e);
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

        private bool ConvertSvgToPngWithInkscape(string svgFile, string pngFile, int width, int height)
        {
            Context.Logger.LogMessage("Inkscape is converting {0} to {1}", svgFile, pngFile);

            string output;
            string command = string.Format("\"{0}\" \"{1}\" -y 0 -w {2} -h {3} -e \"{4}\"",
                InkscapeCom, // 0
                svgFile, // 1
                width.ToString(), // 2
                height.ToString(), // 3
                pngFile // 4
                );

            int ret = Command.Run(command, out output);

            if (ret != 0 || output.IndexOf("CRITICAL **") != -1)
                throw new PipelineException("Error running Inkscape on '{0}'", svgFile);

            return true;
        }

        private bool ConvertSvgToPngWithRSvg(string svgFile, string pngFile, int width, int height)
        {
            Context.Logger.LogMessage("RSvg-Convert is converting {0} to {1}", svgFile, pngFile);

            string output;
            string command = string.Format("\"{0}\" \"{1}\" -w {2} -h {3} -o \"{4}\"",
                PrismProcessor.RSvgConvertExe, // 0
                svgFile, // 1
                width.ToString(), // 2
                height.ToString(), // 3
                pngFile // 4
                );

            int ret = Command.Run(command, out output);

            if (ret != 0)
                throw new PipelineException("Error running RSVG-Convert on '{0}'", svgFile);

            return true;
        }
    }
}