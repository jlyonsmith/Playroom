using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ToyBox
{
#if WINDOWS

    public class MouseArrowComponent : GameComponent
    {
        private ISpriteService spriteService;
        private IInputService inputService;
        private Texture2D mousePointerTexture;
        private TextureSprite mouseSprite;

        public MouseArrowComponent(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            inputService = (IInputService)this.Game.Services.GetService(typeof(IInputService));
            spriteService = (ISpriteService)this.Game.Services.GetService(typeof(ISpriteService));

            this.mousePointerTexture = this.Game.Content.Load<Texture2D>("Textures/Arrow");
            this.mouseSprite = new TextureSprite(
                    new SpriteTexture(this.mousePointerTexture, null),
                    new Point(-this.mousePointerTexture.Width, -this.mousePointerTexture.Height),
                    0, true, null);
            
            spriteService.AttachSprite(this.mouseSprite);

            IMouse mouse = this.inputService.GetMouse();

            mouse.Moved += new MouseMovedDelegate(Mouse_Moved);
        }

        private void Mouse_Moved(Point point)
        {
            this.mouseSprite.Position = point;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.spriteService.DetachSprite(mouseSprite);
            }

            base.Dispose(disposing);
        }
    }

#endif
}
