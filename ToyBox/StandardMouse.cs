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

namespace ToyBox
{
    internal class StandardMouse : IMouse
    {
        private MouseState current;

        public event MouseMoveDelegate MouseMoved;
        public event MouseButtonDelegate MouseButtonPressed;
        public event MouseButtonDelegate MouseButtonReleased;
        public event MouseWheelDelegate MouseWheelRotated;

        public MouseState GetState()
        {
            return current;
        }

        public void MoveTo(Point point)
        {
            throw new NotImplementedException();
        }

        public bool IsAttached
        {
            get { return true; }
        }

        public string Name
        {
            get { return "Mouse"; }
        }

        public void Update()
        {
            MouseState previous = this.current;

            this.current = Mouse.GetState();

            GenerateEvents(ref previous, ref current);
        }

        private void GenerateEvents(ref MouseState previous, ref MouseState current)
        {
            if (MouseMoved == null && MouseButtonPressed == null && MouseButtonReleased == null && MouseWheelRotated == null)
                return;


        }
    }
}
