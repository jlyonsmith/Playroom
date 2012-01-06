using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playroom
{
    public class PrismData
    {
        public IDictionary<string, PrismPinboard> PrismPinboards { get; set; }
    }

    public class PrismPinboard
    {
        public string FileName { get; set; }
        public PinboardData Pinboard { get; set; }
        public IList<PrismPieces> Pieces { get; set; }
    }

    public class PrismPieces
    {
        public string RectangleName { get; set; }
        public string VectorFileName { get; set; }
        public string BitmapFileName { get; set; }
    }
}
