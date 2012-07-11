using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Playroom
{
    public abstract class DxtBitmapContent : BitmapContent
    {
        private int blockSize;
        private byte[] pixelData;

        protected DxtBitmapContent(int blockSize, int width, int height)
            : base(width, height)
        {
            this.blockSize = blockSize;
            this.pixelData = new byte[PixelDataSize(blockSize, width, height)];
        }

        public override byte[] GetPixelData()
        {
            return (byte[])this.pixelData.Clone();
        }

        private static int PixelDataSize(int blockSize, int width, int height)
        {
            width = (width + 3) >> 2;
            height = (height + 3) >> 2;
            return ((width * height) * blockSize);
        }

        public override void SetPixelData(byte[] sourceData)
        {
            this.pixelData = (byte[])sourceData.Clone();
        }

		public int BlockSize { get { return blockSize; } }
    }

    public class Dxt5BitmapContent : DxtBitmapContent
    {
        public Dxt5BitmapContent(int width, int height)
            : base(0x10, width, height)
        {
        }

        public override SurfaceFormat Format
        {
            get { return SurfaceFormat.Dxt5; }
        }
    }
}
