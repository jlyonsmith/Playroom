using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using Playroom;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Playroom.Compilers
{
    class PngToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".png" }; } }
        public string[] OutputExtensions { get { return new string[] { ".xnb" }; } }

        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
		{
			ParsedPath pngFileName = Target.InputFiles.Where(f => f.Extension == ".png").First();
			ParsedPath xnbFileName = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();

			PngFile pngFile = PngFileReader.ReadFile(pngFileName);
			string compressionType;
			SquishMethod? squishMethod = null;
			SurfaceFormat surfaceFormat = SurfaceFormat.Color;

			if (Context.Properties.TryGetValue("CompressionType", out compressionType))
			{
				switch (compressionType.ToLower())
				{
				case "dxt1":
					squishMethod = SquishMethod.Dxt1;
					surfaceFormat = SurfaceFormat.Dxt1;
					break;
				case "dxt3":
					squishMethod = SquishMethod.Dxt3;
					surfaceFormat = SurfaceFormat.Dxt3;
					break;
				case "dxt5":
					squishMethod = SquishMethod.Dxt5;
					surfaceFormat = SurfaceFormat.Dxt5;
					break;
				case "none":
					surfaceFormat = SurfaceFormat.Color;
					break;
				}
			}

			BitmapContent bitmapContent;

			if (squishMethod.HasValue)
			{
				byte[] rgbaData = Squish.CompressImage(
	                pngFile.RgbaData, pngFile.Width, pngFile.Height, 
	                squishMethod.Value, SquishFit.IterativeCluster, SquishMetric.Default, SquishExtra.None);

				bitmapContent = new BitmapContent(surfaceFormat, pngFile.Width, pngFile.Height, rgbaData);
			} 
			else
			{
				bitmapContent = new BitmapContent(SurfaceFormat.Color, pngFile.Width, pngFile.Height, pngFile.RgbaData);
			}

            Texture2DContent textureContent = new Texture2DContent(bitmapContent);

            XnbFileWriterV5.WriteFile(textureContent, xnbFileName);
        }

        #endregion
    }
}
