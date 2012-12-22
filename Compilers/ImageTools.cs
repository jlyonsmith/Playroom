using System;
using ToolBelt;
using System.Collections.Generic;
using Cairo;
using System.IO;

namespace Playroom
{
	public enum ImageRotation 
	{
		None,
		Left,
		Right,
		UpsideDown
	}

	public class ImagePlacement
	{
		public ImagePlacement(ParsedPath pngPath, Cairo.Rectangle targetRectangle)
		{
			this.ImageFile = pngPath;
			this.TargetRectangle = targetRectangle;
		}
		
		public ParsedPath ImageFile { get; set; }
		public Cairo.Rectangle TargetRectangle { get; set; }
	}

	public static class ImageTools
	{
		public static void SvgToPngWithInkscape(string svgFile, string pngFile, int width, int height)
		{
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
				throw new ContentFileException("Error running Inkscape on '{0}': {1}".CultureFormat(svgFile, output));
			}
		}
		
		public static void SvgToPngWithRSvg(string svgFile, string pngFile, int width, int height)
		{
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
				throw new InvalidOperationException("Error running RSVG-Convert on '{0}': {1}".CultureFormat(svgFile, output));
		}

		public static void SvgToPdfWithInkscape(string svgFile, string pdfFile)
		{
			string output;
			string command = string.Format("\"{0}\" --export-pdf=\"{2}\" --file=\"{1}\"",
			                               ToolPaths.Inkscape, // 0
			                               svgFile, // 1
			                               pdfFile); // 2
			
			int ret = Command.Run(command, out output);
			
			if (ret != 0 || output.IndexOf("CRITICAL **") != -1)
			{
				throw new InvalidOperationException("Error running Inkscape on '{0}': {1}".CultureFormat(svgFile, output));
			}
		}

		public static void CombinePngs(List<ImagePlacement> placements, ParsedPath pngPath)
		{
			try
			{
				int w = 0;
				int h = 0;
				
				foreach (var placement in placements)
				{
					int wt = (int)placement.TargetRectangle.Width;
					int ht = (int)placement.TargetRectangle.Height;
					
					if (wt > w)
						w = wt;
					
					if (ht > h)
						h = ht;
				}
				
				using (ImageSurface combinedImage = new ImageSurface(Format.Argb32, w, h))
				{
					using (Cairo.Context g = new Cairo.Context(combinedImage))
					{
						foreach (var placement in placements)
						{
							using (ImageSurface image = new ImageSurface(placement.ImageFile))
							{
								int x = (int)placement.TargetRectangle.X;
								int y = (int)placement.TargetRectangle.Y;
								
								g.SetSourceSurface(image, x, y);
								g.Paint();
							}
						}
					}
					
					combinedImage.WriteToPng(pngPath);
				}
			}
			catch (Exception e)
			{
				throw new ArgumentException("Unable to combine images into file '{0}'".CultureFormat(pngPath), e);
			}
		}

		public static void RotatePng(ParsedPath pngPath, ImageRotation rotation)
		{
			if (rotation == ImageRotation.None)
				return;

			using (ImageSurface originalImage = new ImageSurface(pngPath))
			{
				int w;
				int h;
				
				if (rotation == ImageRotation.Left || rotation == ImageRotation.Right)
				{
					w = originalImage.Height;
					h = originalImage.Width;
				}
				else
				{
					w = originalImage.Width;
					h = originalImage.Height;
				}
				
				double[] rotationRadians = {0, -Math.PI / 2, Math.PI / 2, Math.PI };
				
				using (ImageSurface rotatedImage = new ImageSurface(Format.Argb32, w, h))
				{
					using (Cairo.Context g = new Cairo.Context(rotatedImage))
					{
						g.Translate(rotatedImage.Width / 2.0, rotatedImage.Height / 2.0);
						g.Rotate(rotationRadians[(int)rotation]);
						g.Translate(-originalImage.Width / 2.0, -originalImage.Height / 2.0);
						
						g.SetSourceSurface(originalImage, 0, 0);
						g.Paint();
					}
					
					rotatedImage.WriteToPng(pngPath);
				}
			}
		}

		public static void ExportPngToDxt(ParsedPath pngFileName, ParsedPath dxtFileName)
		{
			PngFile pngFile = PngFileReader.ReadFile(pngFileName);
			SquishMethod? squishMethod = null;

			switch (dxtFileName.Extension)
			{
			case ".dxt1":
				squishMethod = SquishMethod.Dxt1;
				break;
			case ".dxt3":
				squishMethod = SquishMethod.Dxt3;
				break;
			case ".dxt5":
				squishMethod = SquishMethod.Dxt5;
				break;
			default:
				break;
			}

			byte[] rgbaData;

			if (squishMethod.HasValue)
			{
				rgbaData = Squish.CompressImage(
					pngFile.RgbaData, pngFile.Width, pngFile.Height, 
					squishMethod.Value, SquishFit.IterativeCluster, SquishMetric.Default, SquishExtra.None);
			} 
			else
			{
				rgbaData = null;
			}
			
			using (BinaryWriter writer = new BinaryWriter(new FileStream(dxtFileName, FileMode.Create)))
			{
				writer.Write(pngFile.Width);
				writer.Write(pngFile.Height);
				writer.Write(rgbaData);
			}
		}
	}
}

