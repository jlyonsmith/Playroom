using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ToolBelt;
using System.Runtime.InteropServices;

namespace Playroom
{
    public class PixelBitmapContent<T> : BitmapContent
    {
        private SurfaceFormat format;
        private T[][] pixelData;
        private int pixelSize;

        public PixelBitmapContent(SurfaceFormat format, int width, int height) : base(width, height)
        {
            this.format = format;
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
            int stride = pixelSize * base.Width;
            byte[] destinationArray = new byte[stride * base.Height];

            for (int i = 0; i < base.Height; i++)
            {
                GCHandle h = GCHandle.Alloc(this.pixelData[i], GCHandleType.Pinned);
                IntPtr p = (IntPtr)h.AddrOfPinnedObject();
                Marshal.Copy(p, destinationArray, i * stride, stride);
                h.Free();
            }

            return destinationArray;
        }
        
        public override void SetPixelData(byte[] sourceData)
        {
            int stride = pixelSize * base.Width;
            
            this.pixelData = new T[base.Height][];

            for (int i = 0; i < base.Height; i++)
            {
                T[] row = new T[stride];

                GCHandle h = GCHandle.Alloc(row, GCHandleType.Pinned);
                IntPtr p = (IntPtr)h.AddrOfPinnedObject();
                Marshal.Copy(sourceData, i * stride, p, stride);
                h.Free();
                
                this.pixelData[i] = row;
            }
        }

        public override SurfaceFormat Format
        {
            get { return format; }
        }

        public override string ToString()
        {
            return "PixelBitmapContent<{0}>, {1}x{2}".InvariantFormat(typeof(T).Name, base.Width, base.Height);
        }
    }
}
