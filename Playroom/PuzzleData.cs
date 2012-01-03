using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playroom
{
    public class PuzzleData
    {
        public IDictionary<string, PuzzlePinboard> PuzzlePinboards { get; set; }
    }

    public class PuzzlePinboard
    {
        public string FileName { get; set; }
        public PinboardData Pinboard { get; set; }
        public IList<PuzzlePieces> Pieces { get; set; }
    }

    public class PuzzlePieces
    {
        public string RectangleName { get; set; }
        public string VectorFileName { get; set; }
        public string BitmapFileName { get; set; }
    }
}
