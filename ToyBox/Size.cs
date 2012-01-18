using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines a size in 2D space.
    /// </summary>
#if WINDOWS
    [Serializable]
    // TODO: [TypeConverter(typeof(SizeConverter))]
#endif
    public struct Size : IEquatable<Size>
    {
        private static Size zero;

        static Size()
        {
            zero = new Size();
        }

        /// <summary>
        /// Specifies the x-coordinate of the Size.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public int Width;
        /// <summary>
        /// Specifies the y-coordinate of the Size.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public int Height;

        /// <summary>
        /// Initializes a new instance of Size.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Size(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Initializes a new instance of Size.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Size(Vector2 vector)
        {
            this.Width = (int)vector.X;
            this.Height = (int)vector.Y;
        }

        public Size(Point point1, Point point2)
        {
            Point point = point1.Subtract(point2);
            
            this.Width = Math.Abs(point.X);
            this.Height = Math.Abs(point.Y);
        }

        /// <summary>
        /// Determines whether two Size instances are not equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Size a, Size b)
        {
            if (a.Width == b.Width)
            {
                return (a.Height != b.Height);
            }

            return true;
        }

        /// <summary>
        /// Determines whether two Size instances are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Size a, Size b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Returns the size (0, 0)
        /// </summary>
        public static Size Zero 
        { 
            get 
            { 
                return zero; 
            }
        }

        /// <summary>
        /// Determines whether two Size instances are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            bool flag = false;

            if (obj is Size)
            {
                flag = this.Equals((Point)obj);
            }

            return flag;
        }

        /// <summary>
        /// Determines whether two Size instances are equal.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Size other)
        {
            return ((this.Width == other.Width) && (this.Height == other.Height));
        }

        /// <summary>
        /// Gets the hash code for this object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this.Width.GetHashCode() + this.Height.GetHashCode());
        }

        /// <summary>
        /// Returns a String that represents the current Size.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return String.Format(currentCulture, "{{Width:{0} Height:{1}}}", 
                new object[] { this.Width.ToString(currentCulture), this.Height.ToString(currentCulture) });
        }
    }
}
