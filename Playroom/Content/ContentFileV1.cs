using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public class ContentFileV1
    {
        public class Item
        {
            public int LineNumber { get; set; }
            public List<string> InputFiles { get; set; }
            public List<string> OutputFiles { get; set; }
            public List<Tuple<string, string>> Properties { get; set; }
        }

        public List<string> CompilerAssemblyFiles { get; set; }
        public List<ContentFileV1.Item> Items { get; set; }
    }
}
