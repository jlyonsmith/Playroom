using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Jamoki.Games.Spider
{
    public static class RectangleExtensions
    {
        public static Rectangle WithZeroOrigin(this Rectangle rect)
        {
            return new Rectangle(0, 0, rect.Width, rect.Height);
        }

        public static Size GetSpacing(this Rectangle rect, Rectangle other)
        {
            return new Size(Math.Abs(rect.X - other.X), Math.Abs(rect.Y - other.Y));
        }
    }
}
