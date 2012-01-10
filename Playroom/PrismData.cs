using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public class PrismData
    {
        public List<PrismPinboard> Pinboards { get; set; }
    }

    public class PrismPinboard
    {
        public ParsedPath FileName { get; set; }
        public PinboardData Pinboard { get; set; }
        public IList<PrismMapping> Mappings { get; set; }
    }

    public class PrismMapping
    {
        public string RectangleName { get; set; }
        public ParsedPath SvgFileName { get; set; }
        public ParsedPath PngFileName { get; set; }
    }
}
