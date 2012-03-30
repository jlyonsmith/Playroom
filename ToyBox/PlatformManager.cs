using System;
using Microsoft.Xna.Framework;
#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
#elif MONOTOUCH
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

namespace ToyBox
{
	public class PlatformManager : GameComponent, IPlatformService, IDisposable
	{
		#region Fields
		private GamePlatform platform;
		#endregion
		
		#region Construction
		public PlatformManager(Game game) : base(game)
		{
            if (this.Game.Services != null)
            {
                this.Game.Services.AddService(typeof(IPlatformService), this);
            }
		}
		#endregion

		#region IDisposable Implementation
		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if WINDOWS
                this.spriteService.DetachSprite(mouseSprite);
#endif

                if (this.Game.Services != null)
                {
                    this.Game.Services.RemoveService(typeof(IMousePointerService));
                }
            }

            base.Dispose(disposing);
        }
		#endregion

        public override void Initialize()
        {
            base.Initialize();

#if MONOTOUCH
			switch ((int)UIScreen.MainScreen.Bounds.Width)
			{
			case 640:
				platform = GamePlatform.iPhone4;
				break;
			case 320:
				platform = GamePlatform.iPhone3;
				break;
			case 1024:
				platform = GamePlatform.iPad2;
				break;
			case 2048:
				platform = GamePlatform.iPad4;
				break;
			default:
				platform = GamePlatform.Unknown;
				break;
			}
				
#elif WINDOWS
			platform = GamePlatform.Windows7;
#elif WINDOWS_PHONE
			platform = GamePlatform.WindowsPhone7;
#endif
        }


		#region IHardwarePlatformService Implementation
		public GamePlatform Platform { get { return this.platform; } }
		#endregion
	}
}

