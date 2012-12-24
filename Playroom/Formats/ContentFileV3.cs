using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public class ContentFileV3
    {
		public class FilePathGroup
		{
			public string Name { get; set; }
			public string Include { get; set; }
			public string Exclude { get; set; }
		}

        public class Target
        {
			public string Name { get; set; }
            public int LineNumber { get; set; }
			public string Inputs { get; set; }
			public string Outputs { get; set; }
			public string Compiler { get; set; }
            public List<Tuple<string, string>> Properties { get; set; }
        }

        public List<ContentFileV3.FilePathGroup> FilePaths { get; set; }
		public List<Tuple<string, string>> Properties { get; set; }
        public List<ContentFileV3.Target> Targets { get; set; }
    }
}
