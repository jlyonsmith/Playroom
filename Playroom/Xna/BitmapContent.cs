using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using ToolBelt;
using System.Drawing.Imaging;
using Playroom;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class BitmapContent
    {
        private int height;
        private int width;

        protected BitmapContent()
        {
        }

        protected BitmapContent(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public abstract SurfaceFormat Format { get; }
        public abstract byte[] GetPixelData();
        public abstract void SetPixelData(byte[] sourceData);
        
        public override string ToString()
        {
            return "{0}, {1}x{2}".InvariantFormat(base.GetType().Name, this.Width, this.Height);
        }

        private unsafe static byte[] GetRgbaData(Bitmap bitmap)
        {
            byte[] rgba = new byte[4 * bitmap.Width * bitmap.Height];
            BitmapData bitmapData = null;

            try
            {
                bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            rgba[(y * bitmap.Width + x) * 4 + i] = ((byte*)bitmapData.Scan0)[y * bitmapData.Stride + x * 4 + i];
                        }
                    }
                }
            }
            finally
            {
                if (bitmapData != null)
                    bitmap.UnlockBits(bitmapData);
            }

            return rgba;
        }

        public static BitmapContent FromFile(ParsedPath fileName)
        {
            Bitmap bitmap = (Bitmap)Bitmap.FromFile(fileName);

            PixelBitmapContent<Rgba> pixelBitmap = new PixelBitmapContent<Rgba>(SurfaceFormat.Color, bitmap.Width, bitmap.Height);

            pixelBitmap.SetPixelData(GetRgbaData(bitmap));

            return pixelBitmap;
        }

        public int Height
        {
            get
            {
                return this.height;
            }
            private set
            {
                this.height = value;
            }
        }
        public int Width
        {
            get
            {
                return this.width;
            }
            private set
            {
                this.width = value;
            }
        }
    }
}
