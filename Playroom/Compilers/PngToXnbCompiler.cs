using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using Playroom;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            ParsedPath pngFile = Target.InputFiles.Where(f => f.Extension == ".png").First();
            ParsedPath xnbFile = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();

            BitmapContent bitmapContent = BitmapContent.FromFile(pngFile);
            byte[] pixelData;

            // TODO: If requested, DXT compress the image
            pixelData = bitmapContent.GetPixelData();

            pixelData = Squish.CompressImage(
                pixelData, bitmapContent.Width, bitmapContent.Height, 
                SquishMethod.Dxt5, SquishFit.IterativeCluster, SquishMetric.Default, SquishExtra.None);

            Dxt5BitmapContent content = new Dxt5BitmapContent(bitmapContent.Width, bitmapContent.Height);

            content.SetPixelData(pixelData);

            Texture2DContent textureContent = new Texture2DContent();
            
            textureContent.Mipmaps = content;

            XnbFileWriterV5.WriteFile(textureContent, xnbFile);
        }

        #endregion
    }
}
