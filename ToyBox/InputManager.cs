using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ToyBox
{
    public class InputManager : GameComponent, IInputService, IUpdateable, IDisposable 
    {
        private static Keys[] allValidKeys;

        public static Keys[] AllValidKeys
        {
            get
            {
                if (allValidKeys == null)
                    allValidKeys = GetAllValidKeys();

                return allValidKeys;
            }
        }

        private ReadOnlyCollection<IGamePad> gamePads;
        private ReadOnlyCollection<IMouse> mice;
        private ReadOnlyCollection<IKeyboard> keyboards;
        private ReadOnlyCollection<ITouchPanel> touchPanels;

        public InputManager(Game game) :
            this(game, AllValidKeys)
        {
        }

        public InputManager(Game game, Keys[] keysWanted) :
            base(game)
        {
            SetupGamePads();
            SetupMouse();
            SetupKeyboards(keysWanted);
            SetupTouchPanels();

            if (game.Services != null)
            {
                game.Services.AddService(typeof(IInputService), this);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Game.Services != null)
                {
                    this.Game.Services.RemoveService(typeof(IInputService));
                }

                if (this.touchPanels != null)
                {
                    CollectionHelper.DisposeItems(this.touchPanels);
                    this.touchPanels = null;
                }
                if (this.keyboards != null)
                {
                    CollectionHelper.DisposeItems(this.keyboards);
                    this.keyboards = null;
                }
                if (this.mice != null)
                {
                    CollectionHelper.DisposeItems(this.mice);
                    this.mice = null;
                }
                if (this.gamePads != null)
                {
                    CollectionHelper.DisposeItems(this.gamePads);
                    this.gamePads = null;
                }
            }
        }

        public ReadOnlyCollection<IKeyboard> Keyboards
        {
            get { return this.keyboards; }
        }

        public ReadOnlyCollection<IMouse> Mice
        {
            get { return this.mice; }
        }

        public ReadOnlyCollection<IGamePad> GamePads
        {
            get { return this.gamePads; }
        }

        public ReadOnlyCollection<ITouchPanel> TouchPanels
        {
            get { return this.touchPanels; }
        }

        public IMouse GetMouse()
        {
            return CollectionHelper.GetIfExists(this.mice, 0);
        }

        public IKeyboard GetKeyboard()
        {
            return CollectionHelper.GetIfExists(this.keyboards, 4);
        }

        public IKeyboard GetKeyboard(PlayerIndex playerIndex)
        {
            return this.keyboards[(int)playerIndex];
        }

        public IGamePad GetGamePad(PlayerIndex playerIndex)
        {
            return this.gamePads[(int)playerIndex];
        }

        public ITouchPanel GetTouchPanel()
        {
            return this.touchPanels[0];
        }

        public void Update()
        {
            for (int index = 0; index < this.gamePads.Count; ++index)
            {
                this.gamePads[index].Update();
            }
            for (int index = 0; index < this.mice.Count; ++index)
            {
                this.mice[index].Update();
            }
            for (int index = 0; index < this.keyboards.Count; ++index)
            {
                this.keyboards[index].Update();
            }
        }

        private void SetupGamePads()
        {
            var gamePads = new List<IGamePad>();

            // Add default XNA game pads
            for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; ++player)
            {
                gamePads.Add(new XBoxGamePad(player));
            }

            // Add place holders for all unattached game pads
            while (gamePads.Count < 8)
            {
                gamePads.Add(new NoGamePad());
            }

            this.gamePads = new ReadOnlyCollection<IGamePad>(gamePads);
        }

        private void SetupMouse()
        {
            var mice = new List<IMouse>();
#if XBOX360
            // Add a dummy mouse
            mice.Add(new NoMouse());
#else
            // Add main PC mouse
            mice.Add(new StandardMouse());
#endif

            this.mice = new ReadOnlyCollection<IMouse>(mice);
        }

        private void SetupKeyboards(Keys[] keysWanted)
        {
            var keyboards = new List<IKeyboard>();

            for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; ++player)
            {
                keyboards.Add(new GamePadKeyboard(player, this.gamePads[(int)player], keysWanted));
            }
#if XBOX360 || WINDOWS_PHONE
            // Add a dummy keyboard
            keyboards.Add(new NoKeyboard());
#else 
            keyboards.Add(new StandardKeyboard(keysWanted));
#endif
            this.keyboards = new ReadOnlyCollection<IKeyboard>(keyboards);
        }

        private void SetupTouchPanels()
        {
            var touchPanels = new List<ITouchPanel>();

#if WINDOWS_PHONE
            // Add the Windows Phone 7 touch panel
            touchPanels.Add(new XnaTouchPanel());
#else
            touchPanels.Add(new NoTouchPanel());
#endif

            this.touchPanels = new ReadOnlyCollection<ITouchPanel>(touchPanels);
        }

        public static Keys[] GetAllValidKeys()
        {
            FieldInfo[] fieldInfos = typeof(Keys).GetFields(BindingFlags.Public | BindingFlags.Static);

            // Create an array to hold the enumeration values and copy them over from
            // the fields we just retrieved
            var values = new Keys[fieldInfos.Length];

            for (int index = 0; index < fieldInfos.Length; ++index)
            {
                values[index] = (Keys)fieldInfos[index].GetValue(null);
            }

            return values;
        }
    }
}
