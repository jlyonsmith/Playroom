using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
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
        public BuildItem Item { get; set; }

        public void Compile()
        {
            ParsedPath pngFile = Item.InputFiles.Where(f => f.Extension == ".png").First();
            ParsedPath xnbFile = Item.OutputFiles.Where(f => f.Extension == ".xnb").First();

            // TODO: Load the PNG using whatever...
            // TODO: Convert using Squish library

            PixelBitmapContent<Color> pixelBitmap = new PixelBitmapContent<Color>(SurfaceFormat.Color, 0, 0);

            pixelBitmap.SetPixelData(null);

            // TODO: Assign pixel data from squish

            Texture2DContent textureContent = new Texture2DContent();
            
            textureContent.Mipmaps = pixelBitmap;

            XnbFileWriterV5.WriteFile(textureContent, xnbFile);
        }

        #endregion
    }
}
