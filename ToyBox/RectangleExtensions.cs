using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework
{
    public static class RectangleExtensions
    {
        /// <summary>
        /// Create a new rectangle using just the size component of an existing one.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Rectangle FromSize(this Rectangle rect)
        {
            return new Rectangle(0, 0, rect.Width, rect.Height);
        }
    }
}
