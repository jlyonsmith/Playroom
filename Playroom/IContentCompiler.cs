using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public interface IContentCompiler
    {
        IList<CompilerExtension> Extensions { get; }
        BuildContext Context { get; set; }
        BuildTarget Target { get; set; }
        void Compile();
    }
}
