using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.IO;
using System.Xml;
using Cairo;

namespace Playroom
{
    class SvgAndPinboardToXnbConverter : IContentCompiler
    {
		#region Classes
        private class ImagePlacement
        {
            public ImagePlacement(ParsedPath pngFile, Cairo.Rectangle targetRectangle)
            {
                this.ImageFile = pngFile;
                this.TargetRectangle = targetRectangle;
            }

            public ParsedPath ImageFile { get; set; }
            public Cairo.Rectangle TargetRectangle { get; set; }
        }

		#endregion

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
			IEnumerable<ParsedPath> svgFileNames = Target.InputFiles.Where(f => f.Extension == ".svg");
			ParsedPath pinboardFileName = Target.InputFiles.Where(f => f.Extension == ".pinboard").First();
			ParsedPath xnbFileName = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();
			PinboardFileV1 pinboardFile = ReadPinboardFile(pinboardFileName);
			List<ImagePlacement> placements = new List<ImagePlacement>();

			string rectangleName;

			if (this.Context.Properties.TryGetValue("Rectangle", out rectangleName))
				throw new ContentFileException("Rectangle property must be present");

			PinboardFileV1.RectangleInfo rectInfo = pinboardFile.GetRectangleInfoByName(rectangleName);

			if (rectInfo == null)
				throw new ContentFileException("Rectangle {0} not found in pinboard file {1}".CultureFormat(rectangleName, pinboardFileName));

			string converterName;

			if (!this.Context.Properties.TryGetValue("Converter", out converterName))
			{
				converterName = "Inkscape";
			}

			converterName = converterName.ToLower();

			if (converterName != "inkscape" && converterName != "rsvg")
				throw new ContentFileException("Unknown SVG converter '{0}'".CultureFormat(converterName));

			ParsedPath outputRootDir = new ParsedPath(this.Context.Properties.ReplaceVariables(
				this.Context.Properties["OutputRootDir"]), PathType.File);

            try
            {
                // Go through each SVG and output a temporary PNG file.  Create an ImagePlacement for each SVG/PNG processed
				int row = 0;

				foreach (var svgFileName in svgFileNames)
                {
					int col = 0;

                    if (rectInfo == null)
                        throw new InvalidOperationException(
                            "Rectangle '{0}' not found in pinboard '{1}'".CultureFormat(rectangleName, pinboardFileName));

                    ParsedPath pngFile = outputRootDir.SetFileAndExtension(String.Format("{0}_{1}_{2}.png", 
                  		svgFileName, row, col));

                    placements.Add(new ImagePlacement(pngFile,
                        new Rectangle(col * rectInfo.Width, row * rectInfo.Height, rectInfo.Width, rectInfo.Height)));

                    switch (converterName)
                    {
                        default:
                        case "rsvg":
                            ConvertSvgToPngWithRSvg(svgFileName, pngFile, rectInfo.Width, rectInfo.Height);
                            break;

                        case "inkscape":
                            ConvertSvgToPngWithInkscape(svgFileName, pngFile, rectInfo.Width, rectInfo.Height);
                            break;
                    }
                }

                // Combine all the PNG files into the final PNG using the ImagePlacements and delete the temp files.
                CombineImages(placements, xnbFileName.SetExtension(".png"));
            }
            finally
            {
                foreach (var placement in placements)
                {
                    if (File.Exists(placement.ImageFile))
                        File.Delete(placement.ImageFile);
                }
            }
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
                double width = 0;
                double height = 0;

                foreach (var placement in placements)
                {
                    if (placement.TargetRectangle.Width > width)
                        width = placement.TargetRectangle.Width;

                    if (placement.TargetRectangle.Height> height)
                        height = placement.TargetRectangle.Height;
                }

                using (ImageSurface combinedImage = new ImageSurface(Format.Argb32, (int)width, (int)height))
                {
                    using (Cairo.Context g = new Cairo.Context(combinedImage))
                    {
                        foreach (var placement in placements)
                        {
                            using (ImageSurface image = new ImageSurface(placement.ImageFile))
                            {
                                image.SetFallbackResolution(96, 96);

                                g.SetSourceSurface(image, (int)placement.TargetRectangle.X, (int)placement.TargetRectangle.Y);
								g.Paint();
                            }
                        }
                    }

                    combinedImage.WriteToPng(imageFileName);
                }
            }
            catch (Exception e)
            {
                Context.Output.Error("Unable to combine images for image file '{0}'. {1}", imageFileName, e.Message);
                throw;
            }

            return true;
        }

        private bool ConvertSvgToPngWithInkscape(string svgFile, string pngFile, int width, int height)
        {
            Context.Output.Error("Inkscape is converting {0} to {1}", svgFile, pngFile);

            string output;
            string command = string.Format("\"{0}\" --export-png=\"{4}\" --export-width={2} --export-height={3} --export-dpi=96 --file=\"{1}\"",
                ToolPaths.Inkscape, // 0
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
                ToolPaths.RSvgConvert, // 0
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
