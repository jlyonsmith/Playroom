using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using ToolBelt;

namespace Playroom
{
    public class BuildContext
    {
        public BuildContext(OutputHelper output, PropertyGroup properties, ParsedPath contentFile)
        {
            this.Output = output;
            this.Properties = properties;
            this.ContentFile = contentFile;
        }

        public ParsedPath ContentFile { get; set; }
        public OutputHelper Output { get; private set; }
        public PropertyGroup Properties { get; private set; }
		public ItemGroup Items { get; private set; }
    }
}
