using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToyBox
{
    public class BitFlag
    {
        public const int Bit1 = 1 << 0;
        public const int Bit2 = 1 << 1;
        public const int Bit3 = 1 << 2;
        public const int Bit4 = 1 << 3;
        public const int Bit5 = 1 << 4;
        public const int Bit6 = 1 << 5;
        public const int Bit7 = 1 << 6;
        public const int Bit8 = 1 << 7;
        public const int Bit9 = 1 << 8;
        public const int Bit10 = 1 << 9;
        public const int Bit11 = 1 << 10;
        public const int Bit12 = 1 << 11;
        public const int Bit13 = 1 << 12;
        public const int Bit14 = 1 << 13;
        public const int Bit15 = 1 << 14;
        public const int Bit16 = 1 << 15;

        public static IList<int> BitsToPositions(int tags)
        {
            List<int> positions = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                if ((
            }

            return positions;
        }

        public static int PositionsToBits(int IList<int> positions)
        {
            int bits;

            return bits;
        }
    }
}
