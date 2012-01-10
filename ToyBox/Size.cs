using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Xna.Framework
{
    // Summary:
    //     Defines a size in 2D space.
#if WINDOWS
    [Serializable]
    //[TypeConverter(typeof(SizeConverter))]
#endif
    public struct Size : IEquatable<Size>
    {
        private static Size zero;

        static Size()
        {
            zero = new Size();
        }

        // Summary:
        //     Specifies the x-coordinate of the Size.
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public int Width;
        //
        // Summary:
        //     Specifies the y-coordinate of the Size.
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public int Height;

        //
        // Summary:
        //     Initializes a new instance of Size.
        //
        // Parameters:
        //   x:
        //     The x-coordinate of the Size.
        //
        //   y:
        //     The y-coordinate of the Size.
        public Size(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        // Summary:
        //     Determines whether two Size instances are not equal.
        //
        // Parameters:
        //   a:
        //     The Size on the left side of the equal sign.
        //
        //   b:
        //     The Size on the right side of the equal sign.
        public static bool operator !=(Size a, Size b)
        {
            if (a.Width == b.Width)
            {
                return (a.Height != b.Height);
            }

            return true;
        }

        //
        // Summary:
        //     Determines whether two Size instances are equal.
        //
        // Parameters:
        //   a:
        //     Size on the left side of the equal sign.
        //
        //   b:
        //     Size on the right side of the equal sign.
        public static bool operator ==(Size a, Size b)
        {
            return Equals(a, b);
        }

        // Summary:
        //     Returns the point (0,0).
        public static Size Zero 
        { 
            get 
            { 
                return zero; 
            }
        }

        // Summary:
        //     Determines whether two Size instances are equal.
        //
        // Parameters:
        //   obj:
        //     The object to compare this instance to.
        public override bool Equals(object obj)
        {
            bool flag = false;

            if (obj is Size)
            {
                flag = this.Equals((Point)obj);
            }

            return flag;
        }

        //
        // Summary:
        //     Determines whether two Size instances are equal.
        //
        // Parameters:
        //   other:
        //     The Size to compare this instance to.
        public bool Equals(Size other)
        {
            return ((this.Width == other.Width) && (this.Height == other.Height));
        }

        //
        // Summary:
        //     Gets the hash code for this object.
        public override int GetHashCode()
        {
            return (this.Width.GetHashCode() + this.Height.GetHashCode());
        }

        //
        // Summary:
        //     Returns a String that represents the current Size.
        public override string ToString()
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            return String.Format(currentCulture, "{{Width:{0} Height:{1}}}", 
                new object[] { this.Width.ToString(currentCulture), this.Height.ToString(currentCulture) });
        }
    }
}
