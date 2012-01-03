using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public class CrayonData
    {
        public string Namespace { get; set; }
        public List<string> ClassNames { get; set; }
        public List<PlatformData> Platforms { get; set; }
    }

    public class PlatformData
    {
        public string Symbol { get; set; }
        public List<string> FileNames { get; set; }
        public List<PinboardData> Pinboards { get; set; }
    }
}
