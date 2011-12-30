using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;

namespace ToyBox
{
    public interface ISpriteService
    {
        ReadOnlyCollection<Sprite> Sprites { get; }
        TextureSprite AddSprite(SpriteTexture spriteTexture, Point position, int depth, bool visible, object gameObject);
        TextureSprite AddSprite(SpriteTexture[] spriteTextures, int activeTextureIndex, Point position, int depth, bool visible, object gameObject);
        StringSprite AddSprite(SpriteFont font, string text, Point position, int depth, bool visible, object gameObject);
        void DeleteSprite(int index);
        bool AnimationsActive { get; }
        void AttachAnimation(Sprite sprite, Animation animation);
        void DrawSprites();
        int HitTest(Point point);
        RenderTarget2D CreateTexture(int width, int height, IList<TextureAndPosition> textureAndPositions);
    }
}
