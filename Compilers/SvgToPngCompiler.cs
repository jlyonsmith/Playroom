using System;
using System.Collections.Generic;
using ToolBelt;
using System.Linq;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace Playroom
{
	public class SvgToPngCompiler : IContentCompiler
	{
		#region Fields
		private CompilerExtension[] extensions = new CompilerExtension[]
		{
			new CompilerExtension(".svg", ".png")
		};
		#endregion 
		
		#region IContentCompiler
		public CompilerExtension[] Extensions { get { return extensions; } }
		public BuildContext Context { get; set; }
		public BuildTarget Target { get; set; }

		public void Setup(YamlMappingNode settings)
		{
		}
		
		public void Compile()
		{
			ParsedPath svgFileName = Target.InputPaths.Where(f => f.Extension == ".svg").First();
			ParsedPath pngFileName = Target.OutputPaths.Where(f => f.Extension == ".png").First();

			int width, height;

			Target.Properties.GetRequiredValue("Width", out width);
			Target.Properties.GetRequiredValue("Height", out height);

			if (!Directory.Exists(pngFileName.VolumeAndDirectory))
			{
				Directory.CreateDirectory(pngFileName.VolumeAndDirectory);
			}

			ImageTools.SvgToPngWithInkscape(svgFileName, pngFileName, width, height);
		}

		#endregion
	}
}

