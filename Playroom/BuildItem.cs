using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public class BuildItem
    {
        public int LineNumber { get; internal set; }
        public IList<ParsedPath> InputFiles { get; internal set; }
        public IEnumerable<string> InputExtensions { get; internal set; }
        public IList<ParsedPath> OutputFiles { get; internal set; }
        public IEnumerable<string> OutputExtensions { get; internal set; }
        public PropertyCollection Properties { get; internal set; }
    }
}
