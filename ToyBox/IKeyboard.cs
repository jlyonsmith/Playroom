using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace ToyBox
{
    public delegate void KeyDelegate(Keys key);
    public delegate void CharacterDelegate(char character);

    public interface IKeyboard : IInputDevice
    {
        event KeyDelegate KeyPressed;
        event KeyDelegate KeyReleased;
        event CharacterDelegate CharacterEntered;
        KeyboardState GetState();
    }
}
