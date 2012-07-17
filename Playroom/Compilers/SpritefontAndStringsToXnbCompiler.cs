using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;
using Microsoft.Xna.Framework;

namespace Playroom
{
    public class SpritefontAndStringsToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".spritefont", ".strings" }; } }
        public string[] OutputExtensions { get { return new string[] { ".xnb" }; } }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
            ParsedPath spritefontFile = Target.InputFiles.Where(f => f.Extension == ".spritefont").First();
            ParsedPath stringsFile = Target.InputFiles.Where(f => f.Extension == ".strings").First();
            ParsedPath xnbFile = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();
        }

        #endregion
    }
}
