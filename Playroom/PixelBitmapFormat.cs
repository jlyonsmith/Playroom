using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ToolBelt;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class PixelBitmapContent<T> : BitmapContent
    {
        private T[][] pixelData;
        private int pixelSize;

        public PixelBitmapContent(SurfaceFormat format, int width, int height) : base(format, width, height)
        {
            this.pixelData = new T[height][];
            this.pixelSize = Marshal.SizeOf(typeof(T));

            for (int i = 0; i < height; i++)
            {
                this.pixelData[i] = new T[width];
            }
        }

        public T GetPixel(int x, int y)
        {
            return this.pixelData[y][x];
        }

        public T[] GetRow(int y)
        {
            return this.pixelData[y];
        }

        public void ReplaceColor(T originalColor, T newColor)
        {
            foreach (T[] localArray in this.pixelData)
            {
                for (int i = 0; i < localArray.Length; i++)
                {
                    if (localArray[i].Equals(originalColor))
                    {
                        localArray[i] = newColor;
                    }
                }
            }
        }

        public void SetPixel(int x, int y, T value)
        {
            this.pixelData[y][x] = value;
        }

        public override byte[] GetPixelData()
        {
            int num2 = pixelSize * base.Width;
            byte[] destinationArray = new byte[num2 * base.Height];

            for (int i = 0; i < base.Height; i++)
            {
            }

            return destinationArray;
        }
        
        public override void SetPixelData(byte[] sourceData)
        {
            int count = pixelSize * base.Width;

            for (int i = 0; i < base.Height; i++)
            {
                // TODO: ...
            }
        }

        public override string ToString()
        {
            return "PixelBitmapContent<{0}>, {1}x{2}".InvariantFormat(typeof(T).Name, base.Width, base.Height);
        }
    }
}
