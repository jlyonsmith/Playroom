using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ToyBox
{
#if WINDOWS

    public class MouseArrowComponent : DrawableGameComponent
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

            this.mouseSprite = spriteService.AddSprite(
                new SpriteTexture(this.mousePointerTexture, null),
                new Point(-this.mousePointerTexture.Width, -this.mousePointerTexture.Height),
                0, true, null);

            IMouse mouse = this.inputService.GetMouse();

            mouse.MouseMoved += new MouseMoveDelegate(OnMouseMoved);
        }

        private void OnMouseMoved(Point point)
        {
            this.mouseSprite.Position = point;
        }
    }

#endif
}
