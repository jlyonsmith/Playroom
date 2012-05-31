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
        void Compile(BuildContext buildContext, BuildItem buildItem);
    }
}
