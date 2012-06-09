using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ToolBelt;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class BitmapContent
    {
        private int height;
        private int width;
        private SurfaceFormat format;

        protected BitmapContent()
        {
        }

        protected BitmapContent(SurfaceFormat format, int width, int height)
        {
            this.format = format;
            this.Width = width;
            this.Height = height;
        }

        public abstract byte[] GetPixelData();
        public abstract void SetPixelData(byte[] sourceData);
        
        public override string ToString()
        {
            return "{0}, {1}x{2}".InvariantFormat(base.GetType().Name, this.Width, this.Height);
        }

        public SurfaceFormat Format { get { return format; } }
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
