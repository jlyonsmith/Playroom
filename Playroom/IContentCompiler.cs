using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using YamlDotNet.RepresentationModel;

namespace Playroom
{
    public interface IContentCompiler
    {
        CompilerExtension[] Extensions { get; }
        BuildContext Context { get; set; }
        BuildTarget Target { get; set; }
		void Setup(YamlMappingNode settings);
        void Compile();
    }
}
