using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public class PinataData
    {
        public string Namespace { get; set; }
        public String Symbol { get; set; }
        public List<PinataClassData> Classes { get; set; }
    }

    public class PinataClassData
    {
        public string Prefix { get; set; }
        public ParsedPath PinboardFile { get; set; }
        public PinboardData Pinboard { get; set; }
    }
}
