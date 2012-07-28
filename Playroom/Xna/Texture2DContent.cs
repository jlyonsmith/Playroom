using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class Texture2DContent : TextureContent
    {
        public Texture2DContent() : base(new MipmapChainCollection(1))
        {
        }

        public MipmapChain Mipmaps
        {
            get
            {
                return base.Faces[0];
            }
            set
            {
                base.Faces[0] = value;
            }
        }
    }
}
