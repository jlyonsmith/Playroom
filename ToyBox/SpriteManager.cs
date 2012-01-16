using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

    public class SpriteManager : GameComponent, ISpriteService
    {
        private List<Animation> activeAnimations;
        private List<Sprite> sprites;
        private SpriteBatch spriteBatch;
        private ReadOnlyCollection<Sprite> readOnlySprites;
        
        public ReadOnlyCollection<Sprite> Sprites { get { return readOnlySprites; } }
        public GraphicsDevice GraphicsDevice { get { return this.Game.GraphicsDevice; } }

        public SpriteManager(Game game)
            : base(game)
        {
            sprites = new List<Sprite>();
            readOnlySprites = sprites.AsReadOnly();
            activeAnimations = new List<Animation>();

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
            for (int i = 0; i < activeAnimations.Count; )
            {
                Animation animation = this.activeAnimations[i];

                animation.Update(gameTime);

                if (animation.HasFinished)
                    activeAnimations.RemoveAt(i);
                else
                    i++;
            }

            base.Update(gameTime);
        }

        public bool AnimationsActive
        {
            get
            {
                return activeAnimations.Count > 0;
            }
        }

        public TextureSprite AddSprite(SpriteTexture spriteTexture, Point position, int depth, bool visible, object gameObject)
        {
            SpriteTexture[] spriteTextures = new SpriteTexture[1];

            spriteTextures[0] = spriteTexture;
            
            TextureSprite sprite = new TextureSprite(spriteTextures, 0, position, depth, visible, gameObject, sprites.Count);

            this.sprites.Add(sprite);

            return sprite;
        }

        public TextureSprite AddSprite(SpriteTexture[] spriteTextures, int activeTextureIndex, Point position, int depth, bool visible, object gameObject)
        {
            TextureSprite sprite = new TextureSprite(spriteTextures, activeTextureIndex, position, depth, visible, gameObject, sprites.Count);

            this.sprites.Add(sprite);

            return sprite;
        }

        public StringSprite AddSprite(SpriteFont font, string text, Point position, int depth, bool visible, object gameObject)
        {
            StringSprite sprite = new StringSprite(font, text, position, depth, visible, gameObject, sprites.Count);

            this.sprites.Add(sprite);

            return sprite;
        }

        public void AttachAnimation(Sprite sprite, Animation animation)
        {
            animation.OnInitialize(this, sprite);

            Animation activeAnimation = sprite.ActiveAnimation;

            if (activeAnimation == null)
            {
                sprite.ActiveAnimation = animation;
                this.activeAnimations.Add(animation);
            }
            else
            {
                while (activeAnimation.NextAnimation != null)
                {
                    activeAnimation = activeAnimation.NextAnimation;
                }

                activeAnimation.NextAnimation = animation;
            }
        }

        public void ActivateNextAnimation(Sprite sprite, Animation nextAnimation)
        {
            this.activeAnimations.Add(nextAnimation);
        }

        public void DeleteSprite(int index)
        {
            // TODO-john-2011: Remove any animations for this sprite too?

            TextureSprite textureSprite = this.sprites[index] as TextureSprite;

            if (textureSprite != null && textureSprite.OwnsTextures)
            {
                foreach (var spriteTexture in textureSprite.SpriteTextures)
                {
                    spriteTexture.Texture.Dispose();
                }
            }

            this.sprites.RemoveAt(index);
        }

        public void DrawSprites()
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
    }
}
