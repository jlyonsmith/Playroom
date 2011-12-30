using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections;
using System.Collections.ObjectModel;

namespace ToyBox
{
    public abstract class Sprite
    {
        public static readonly int MaxDepth = UInt16.MaxValue;

        protected float xnaDepth;
        protected int depth;
        
        public Point Position { get; set; }
        public float Rotation { get; set; }
        public abstract Rectangle Box { get; }
        public bool Visible { get; set; }
        public int Depth 
        {
            get
            {
                return depth;
            }
            set
            {
                depth = Math.Max(0, Math.Min(value, MaxDepth));
                xnaDepth = (float)depth / (float)MaxDepth;
            }
        }
        public object GameObject { get; set; }
        public bool HitTestable { get { return this.GameObject != null; } }
        public Vector2 Scale { get; set; }
        public Animation ActiveAnimation { get; set; }
        public int SpriteIndex { get; private set; }

        public Sprite(Point position, int depth, bool visible, object gameObject, int index)
        {
            this.Position = position;
            this.Visible = visible;
            this.Depth = depth;
            this.Scale = Vector2.One;
            this.ActiveAnimation = null;
            this.GameObject = gameObject;
            this.SpriteIndex = index;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }

    public class TextureSprite : Sprite
    {
        public ReadOnlyCollection<SpriteTexture> SpriteTextures { get; private set; }
        public int ActiveTextureIndex { get; set; }
        public override Rectangle Box
        {
            get
            {
                Rectangle textureRect = this.SpriteTextures[this.ActiveTextureIndex].Rectangle;

                return new Rectangle(this.Position.X, this.Position.Y, textureRect.Width, textureRect.Height);
            }
        }

        public TextureSprite(SpriteTexture[] spriteTextures, int activeTextureIndex, Point position, int depth, bool visible, object gameObject, int index)
            : base(position, depth, visible, gameObject, index)
        {
            this.SpriteTextures = Array.AsReadOnly<SpriteTexture>(spriteTextures);
            this.ActiveTextureIndex = activeTextureIndex;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            SpriteTexture texture = this.SpriteTextures[ActiveTextureIndex];

            spriteBatch.Draw(
                texture.Texture,
                new Vector2(Position.X, Position.Y),
                texture.Rectangle,
                Color.White,
                this.Rotation,
                Vector2.Zero,
                this.Scale,
                SpriteEffects.None,
                xnaDepth);
        }
    }

    public class StringSprite : Sprite
    {
        public SpriteFont Font { get; set; }
        public String Text { get; set; }
        public override Rectangle Box
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public StringSprite(SpriteFont font, string text, Point position, int depth, bool visible, object gameObject, int index)
            : base(position, depth, visible, gameObject, index)
        {
            this.Font = font;
            this.Text = text;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(
                this.Font, 
                this.Text,
                new Vector2(Position.X, Position.Y),
                Color.White,
                this.Rotation,
                Vector2.Zero,
                this.Scale,
                SpriteEffects.None,
                xnaDepth);
        }
    }
}
