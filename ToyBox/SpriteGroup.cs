using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ToyBox
{
    public class SpriteGroup
    {
        private List<Sprite> sprites;
        private ReadOnlyCollection<Sprite> readOnlySprites;

        public SpriteGroup()
        {
        }

        public void AttachSprite(Sprite sprite)
        {
            if (sprites == null)
            {
                sprites = new List<Sprite>();
                readOnlySprites = sprites.AsReadOnly();
            }

            sprites.Add(sprite);
        }
    }
}
