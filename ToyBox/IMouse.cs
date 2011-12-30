using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ToyBox
{
    public delegate void MouseMoveDelegate(Point point);
    public delegate void MouseButtonDelegate(MouseButtons buttons);
    public delegate void MouseWheelDelegate(float ticks);

    public interface IMouse : IInputDevice
    {
        event MouseMoveDelegate MouseMoved;
        event MouseButtonDelegate MouseButtonPressed;
        event MouseButtonDelegate MouseButtonReleased;
        event MouseWheelDelegate MouseWheelRotated;
        MouseState GetState();
        void MoveTo(Point point);
    }
}
