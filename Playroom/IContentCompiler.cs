using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public interface IContentCompiler
    {
        string[] InputExtensions { get; }
        string[] OutputExtensions { get; }
        BuildContext Context { get; set; }
        BuildItem Item { get; set; }
        void Compile();
    }
}
