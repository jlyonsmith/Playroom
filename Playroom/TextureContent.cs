using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ToolBelt;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Playroom
{
    public abstract class TextureContent
    {
        private MipmapChainCollection faces;

        protected TextureContent(MipmapChainCollection faces)
        {
            this.faces = faces;
        }

        internal static string FormatTextureSize(BitmapContent bitmap)
        {
            return "{0}x{1}".InvariantFormat(bitmap.Width, bitmap.Height);
        }

        public MipmapChainCollection Faces
        {
            get
            {
                return this.faces;
            }
        }
    }
}
