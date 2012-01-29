using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using ToolBelt;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

using TInput = Playroom.PrismData;
using TOutput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.TextureContent;

namespace Playroom
{
    [ContentProcessor(DisplayName = "Prism Data")]
    public class PrismDataProcessor : ContentProcessor<TInput, TOutput>
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

        private static ParsedPath InkscapeExe { get; set; }
        private ContentProcessorContext Context { get; set; }

        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            ParsedPath intermediateDir = new ParsedPath(context.IntermediateDirectory, PathType.Directory);

            this.Context = context;

            // TODO-john-2012: Uhhh
            
            InkscapeExe = new ParsedPath("inkscape.exe", PathType.File);

            // Make all .SVG paths absolute relative to the .PRISM file and add dependencies on them
            for (int i = 0; i < input.SvgFiles.Count; i++)
            {
                List<ParsedPath> list = input.SvgFiles[i];

                for (int j = 0; j < list.Count; j++)
                {
                    list[j] = list[j].MakeFullPath(input.PrismFile);

                    context.AddDependency(list[j]);
                }
            }

            // Make .PINBOARD path absolute and add a dependency
            input.PinboardFile = input.PinboardFile.MakeFullPath(input.PrismFile);
            context.AddDependency(input.PinboardFile);

            // TODO-john-2012: Go through each SVG and output a temporary PNG file.  Create an ImagePlacement for each SVG/PNG processed
            ConvertSvgToPng("", "", 0, 0);

            // TODO-john-2012: If there is just one PNG file, rename it the final PNG.

            // TODO-john-2012: For multiple output files. Combine all the PNG files into the final PNG using the ImagePlacements.  Delete all temporary PNG files

            OpaqueDataDictionary processorParams = new OpaqueDataDictionary();
            ExternalReference<TextureContent> exRef = new ExternalReference<TextureContent>(input.PngFile);
            TextureContent textureContent = context.BuildAndLoadAsset<TextureContent, TextureContent>(exRef, null, processorParams, null);

            return textureContent;
        }

        private bool CombineImages(List<ImagePlacement> placements, ParsedPath imageFileName)
        {
            try
            {
                int width = 0;
                int height = 0;
                //StringBuilder sb = new StringBuilder();

                foreach (var placement in placements)
                {
                    if (placement.TargetRectangle.Right > width)
                        width = placement.TargetRectangle.Right;

                    if (placement.TargetRectangle.Bottom > height)
                        height = placement.TargetRectangle.Bottom;

                    //sb.AppendFormat(placement.ImageFile + Environment.NewLine);
                }

                using (Bitmap combinedImage = new Bitmap(width, height))
                {
                    using (Graphics g = Graphics.FromImage(combinedImage))
                    {
                        foreach (var placement in placements)
                        {
                            using (Bitmap image = new Bitmap(placement.ImageFile))
                            {
                                g.DrawImage(image, new System.Drawing.Point(placement.TargetRectangle.X, placement.TargetRectangle.Y));
                            }
                        }
                    }

                    SavePng(combinedImage, imageFileName);
                }

                //sb.AppendFormat("-> {0}", imageFileName);
            }
            catch (Exception e)
            {
                throw new InvalidContentException("Unable to combine images", e);
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

        private bool ConvertSvgToPng(string svgFile, string pngFile, int width, int height)
        {
            string output;
            string command = string.Format("\"{0}\" \"{1}\" -w {2} -h {3} -o \"{4}\"",
                InkscapeExe, // 0
                svgFile, // 1
                width.ToString(), // 2
                height.ToString(), // 3
                pngFile // 4
                );

            int ret = Command.Run(command, out output);

            if (ret != 0)
            {
                // TODO-john-2012: Error message
                return false;
            }

            return true;
        }
    }
}