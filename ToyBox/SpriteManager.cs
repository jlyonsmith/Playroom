using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace ToyBox
{
    public class TextureAndPosition
    {
        public TextureAndPosition(Texture2D texture, int x, int y, Rectangle? sourceRectangle)
        {
            this.Texture = texture;
            this.SourceRectangle = sourceRectangle.HasValue ? 
                sourceRectangle.Value : new Rectangle(0, 0, texture.Width, texture.Height);
            this.Position = new Vector2((float)x, (float)y);
        }

        public Texture2D Texture { get; private set; }
        public Rectangle SourceRectangle { get; private set; }
        public Vector2 Position { get; private set; } 
    }

    public class SpriteEventArgs : EventArgs
    {
        public SpriteEventArgs(Sprite sprite)
        {
            this.Sprite = sprite;
        }

        public Sprite Sprite { get; private set; }
    }

    public class SpriteManager : GameComponent, ISpriteService
    {
        private SpriteBatch spriteBatch;
        private List<Animation> animations;
        private ReadOnlyCollection<Animation> readOnlyAnimations;
        private List<Sprite> sprites;
        private ReadOnlyCollection<Sprite> readOnlySprites;
        
        public ReadOnlyCollection<Sprite> Sprites { get { return readOnlySprites; } }
        public ReadOnlyCollection<Animation> Animations { get { return readOnlyAnimations; } }

        public GraphicsDevice GraphicsDevice { get { return this.Game.GraphicsDevice; } }

        public SpriteManager(Game game)
            : base(game)
        {
            sprites = new List<Sprite>();
            readOnlySprites = sprites.AsReadOnly();
            animations = new List<Animation>();
            readOnlyAnimations = animations.AsReadOnly();

            if (this.Game.Services != null)
            {
                this.Game.Services.AddService(typeof(ISpriteService), this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Game.Services != null)
                {
                    this.Game.Services.RemoveService(typeof(ISpriteService));
                }
            }

            base.Dispose(disposing);
        }

        public override void Initialize()
        {
            base.Initialize();

            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < animations.Count; )
            {
                Animation animation = this.animations[i];

                animation.Update(gameTime);

                if (animation.HasFinished)
                    animations.RemoveAt(i);
                else
                    i++;
            }

            base.Update(gameTime);
        }

        public void AttachSprite(Sprite sprite)
        {
            this.sprites.Add(sprite);
        }

        public void AttachAnimation(Sprite sprite, Animation animation)
        {
            animation.Initialize(new ActivateNextAnimationDelegate(Animation_ActivateNextAnimation), sprite);

            Animation activeAnimation = sprite.ActiveAnimation;

            if (activeAnimation == null)
            {
                sprite.ActiveAnimation = animation;
                this.animations.Add(animation);
            }
            else
            {
                while (activeAnimation.NextAnimation != null)
                {
                    activeAnimation = activeAnimation.NextAnimation;

                    if (activeAnimation == animation)
                        throw new ArgumentException("Same animation attached more than once");
                }

                activeAnimation.NextAnimation = animation;
            }
        }

        public void DetachSprite(Sprite sprite)
        {
            int index = this.sprites.IndexOf(sprite);

            TextureSprite textureSprite = sprite as TextureSprite;

            if (textureSprite != null && textureSprite.OwnsTextures)
            {
                foreach (var spriteTexture in textureSprite.SpriteTextures)
                {
                    spriteTexture.Texture.Dispose();
                }
            }

            this.sprites.RemoveAt(index);
        }

        public void Draw()
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            foreach (var sprite in sprites)
            {
                if (sprite.Visible)
                    sprite.Draw(spriteBatch);
            }

            spriteBatch.End();
        }

        public int HitTest(Point point)
        {
            int minDepthFound = Sprite.MaxDepth;
            int foundIndex = -1;

            for (int i = 0; i < sprites.Count; i++)
            {
                Sprite sprite = sprites[i];

                if (sprite.Visible && 
                    sprite.HitTestable &&
                    sprite.Rectangle.Contains(point) && 
                    sprite.Depth < minDepthFound)
                {
                    foundIndex = i;
                    minDepthFound = sprite.Depth;
                }
            }

            return foundIndex;
        }

        public RenderTarget2D CreateTexture(int width, int height, IList<TextureAndPosition> textureAndPositions)
        {
            RenderTarget2D target = new RenderTarget2D(
                this.Game.GraphicsDevice, width, height,
                false, SurfaceFormat.Color, DepthFormat.None);

            this.Game.GraphicsDevice.SetRenderTarget(target);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            foreach (var textureAndPosition in textureAndPositions)
            {
                spriteBatch.Draw(textureAndPosition.Texture, textureAndPosition.Position,
                    textureAndPosition.SourceRectangle, Color.White);
            }

            spriteBatch.End();

            this.Game.GraphicsDevice.SetRenderTarget(null);

            return target;
        }

        private void Animation_ActivateNextAnimation(Animation animation, Animation nextAnimation)
        {
            this.animations.Add(nextAnimation);
        }
    }
}
