using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.Collections;

namespace ToyBox
{
    public interface ISpriteService
    {
        ReadOnlyCollection<Sprite> Sprites { get; }
        ReadOnlyCollection<Animation> Animations { get; }
        void AttachSprite(Sprite sprite);
        void DetachSprite(Sprite sprite);
        void AttachAnimation(Sprite sprite, Animation animation);
        void Draw();
        int HitTest(Point point);
        RenderTarget2D CreateTexture(int width, int height, IList<TextureAndPosition> textureAndPositions);
    }
}
