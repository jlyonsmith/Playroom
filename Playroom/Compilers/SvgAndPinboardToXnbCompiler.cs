using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using Microsoft.Win32;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;

namespace Playroom
{
    class SvgAndPinboardToXnbConverter : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".svg", ".pinboard" }; }
        }

        public string[] OutputExtensions
        {
            get { return new string[] { ".xnb" }; }
        }

        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
            /*
            ParsedPath prismFile = new ParsedPath(fileName, PathType.File);

            if (!File.Exists(prismFile))
            {
                throw new FileNotFoundException(PlayroomResources.FileNotFound(prismFile));
            }

            PrismData pinataData = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(prismFile))
                {
                    pinataData = PrismDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(String.Format("Unable to read prism data. {0}", e.Message), new ContentIdentity(fileName), e);
            }

            pinataData.PrismFile = prismFile;
            pinataData.PngFile = new ParsedPath(context.IntermediateDirectory, PathType.Directory).SetFileAndExtension(prismFile.File + ".png");
            pinataData.PinboardFile = pinataData.PinboardFile.MakeFullPath(prismFile);

            if (pinataData.SvgDirectory != null)
                pinataData.SvgDirectory = pinataData.SvgDirectory.MakeFullPath(prismFile);

            ParsedPath intermediateDir = new ParsedPath(context.IntermediateDirectory, PathType.Directory);

            this.Context = context;
            this.PrismFile = pinataData.PrismFile;

            // Make all .svg paths absolute and add dependencies on them
            for (int i = 0; i < pinataData.SvgFiles.Count; i++)
            {
                List<ParsedPath> list = pinataData.SvgFiles[i];

                for (int j = 0; j < list.Count; j++)
                {
                    list[j] = list[j].MakeFullPath(pinataData.SvgDirectory == null ? pinataData.PrismFile : pinataData.SvgDirectory);

                    context.AddDependency(list[j]);
                }
            }

            // Grab the pinboard data and add a dependency
            pinataData.Pinboard = ReadPinboardFile(pinataData.PinboardFile);
            context.AddDependency(pinataData.PinboardFile);

            ParsedPath tmpPath = new ParsedPath(context.IntermediateDirectory, PathType.Directory);

            List<ImagePlacement> placements = new List<ImagePlacement>();

            try
            {
                // Go through each SVG and output a temporary PNG file.  Create an ImagePlacement for each SVG/PNG processed
                for (int row = 0; row < pinataData.SvgFiles.Count; row++)
                {
                    List<ParsedPath> pathList = pinataData.SvgFiles[row];

                    for (int col = 0; col < pathList.Count; col++)
                    {
                        RectangleInfo rectInfo = pinataData.Pinboard.GetRectangleInfoByName(pinataData.RectangleName);

                        if (rectInfo == null)
                            throw new InvalidOperationException(
                                String.Format("Rectangle '{0}' not found in pinboard '{1}'", pinataData.RectangleName, pinataData.PinboardFile),
                                new ContentIdentity(pinataData.PrismFile));

                        ParsedPath tmpPngFile = tmpPath.SetFileAndExtension(String.Format("{0}_{1}_{2}.png", pinataData.PngFile.File, row, col));

                        placements.Add(new ImagePlacement(tmpPngFile,
                            new Rectangle(col * rectInfo.Width, row * rectInfo.Height, rectInfo.Width, rectInfo.Height)));

                        if (!File.Exists(pathList[col]))
                            throw new InvalidOperationException(
                                PlayroomResources.FileNotFound(pathList[col]), new ContentIdentity(pinataData.PrismFile));

                        switch (pinataData.Converter)
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
                CombineImages(placements, pinataData.PngFile);
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

            processorParams["ColorKeyEnabled"] = false;
            processorParams["PremultiplyAlpha"] = true;
            processorParams["GenerateMipmaps"] = false;
            processorParams["TextureFormat"] = TextureProcessorOutputFormat.Color;

            ExternalReference<TextureContent> exRef = new ExternalReference<TextureContent>(pinataData.PngFile);
            TextureContent textureContent = context.BuildAndLoadAsset<TextureContent, TextureContent>(exRef, "TextureProcessor", processorParams, "TextureImporter");

            return textureContent;
            */
        }

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

        private static Dictionary<ParsedPath, PinboardFileV1> pinboardDataCache;

        private PinboardFileV1 ReadPinboardFile(ParsedPath pinboardFile)
        {
            PinboardFileV1 data = null;

            if (pinboardDataCache == null)
                pinboardDataCache = new Dictionary<ParsedPath, PinboardFileV1>();

            if (pinboardDataCache.TryGetValue(pinboardFile, out data))
            {
                return data;
            }

            try
            {
                data = PinboardFileReaderV1.ReadFile(pinboardFile);
            }
            catch
            {
                Context.Output.Error(String.Format("Unable to read pinboard file '{0}'", pinboardFile));
                throw;
            }

            pinboardDataCache.Add(pinboardFile, data);

            return data;
        }

        private bool CombineImages(List<ImagePlacement> placements, ParsedPath imageFileName)
        {
            Context.Output.Message("Combining images into {0}", imageFileName);

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
                                image.SetResolution(96, 96);

                                g.DrawImageUnscaled(image, placement.TargetRectangle);
                            }
                        }
                    }

                    SavePng(combinedImage, imageFileName);
                }
            }
            catch (Exception e)
            {
                Context.Output.Error("Unable to combine images for image file '{0}'. {1}", imageFileName, e.Message);
                throw;
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
            Context.Output.Error("Inkscape is converting {0} to {1}", svgFile, pngFile);

            string output;
            string command = string.Format("\"{0}\" --export-png=\"{4}\" --export-width={2} --export-height={3} --export-dpi=96 --file=\"{1}\"",
                ToolPaths.InkscapeCom, // 0
                svgFile, // 1
                width.ToString(), // 2
                height.ToString(), // 3
                pngFile // 4
                );

            int ret = Command.Run(command, out output);

            if (ret != 0 || output.IndexOf("CRITICAL **") != -1)
            {
                throw new ContentFileException(Context.ContentFile, Target.LineNumber, String.Format("Error running Inkscape on '{0}'", svgFile));
            }

            return true;
        }

        private bool ConvertSvgToPngWithRSvg(string svgFile, string pngFile, int width, int height)
        {
            Context.Output.Message("RSvg-Convert is converting {0} to {1}", svgFile, pngFile);

            string output;
            string command = string.Format("\"{0}\" \"{1}\" --format=png --dpi-x=96 --dpi-y=96 --width={2} --height={3} --output \"{4}\"",
                ToolPaths.RSvgConvertExe, // 0
                svgFile, // 1
                width.ToString(), // 2
                height.ToString(), // 3
                pngFile // 4
                );

            int ret = Command.Run(command, out output);

            if (ret != 0)
                throw new ContentFileException(Context.ContentFile, Target.LineNumber, String.Format("Error running RSVG-Convert on '{0}'", svgFile));

            return true;
        }

        #endregion
    }
}
