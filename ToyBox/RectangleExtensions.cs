﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Jamoki.Games.Spider
{
    public static class RectangleExtensions
    {
        /// <summary>
        /// Gets the same rectangle with a zero origin.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Rectangle WithZeroOrigin(this Rectangle rect)
        {
            return new Rectangle(0, 0, rect.Width, rect.Height);
        }

        /// <summary>
        /// Gets the spacing defined by the rectangle and another, defined as the absolute horizontal and vertical
        /// distance between the two rectangles top-left corners.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Size Spacing(this Rectangle rect, Rectangle other)
        {
            return new Size(Math.Abs(rect.X - other.X), Math.Abs(rect.Y - other.Y));
        }
    }
}