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
        public BuildContext(OutputHelper output, ParsedPath contentFilePath)
        {
            this.Output = output;
            this.ContentFilePath = contentFilePath;
        }

        public ParsedPath ContentFilePath { get; private set; }
        public OutputHelper Output { get; private set; }
    }
}
