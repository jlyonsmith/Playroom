using System;
using System.Collections.Generic;
using ToolBelt;
using System.Linq;
using System.IO;

namespace Playroom
{
	public class SvgToPngCompiler : IContentCompiler
	{
		#region Construction
		public SvgToPngCompiler()
		{
		}
		#endregion

		#region Fields
		private CompilerExtension[] extensions = new CompilerExtension[]
		{
			new CompilerExtension(".svg", ".png")
		};
		#endregion 
		
		#region Properties
		[ContentCompilerParameterAttribute("Width of the bitmap in pixels", Optional = false)]
		public int Width { get; set; }
		
		[ContentCompilerParameterAttribute("Height of the bitmap in pixels", Optional = false)]
		public int Height { get; set; }
		#endregion
		
		#region IContentCompiler
		public IList<CompilerExtension> Extensions { get { return extensions; } }
		public BuildContext Context { get; set; }
		public BuildTarget Target { get; set; }

		public void Compile()
		{
			ParsedPath svgFileName = Target.InputPaths.Where(f => f.Extension == ".svg").First();
			ParsedPath pngFileName = Target.OutputPaths.Where(f => f.Extension == ".png").First();

			if (!Directory.Exists(pngFileName.VolumeAndDirectory))
			{
				Directory.CreateDirectory(pngFileName.VolumeAndDirectory);
			}

			ImageTools.SvgToPngWithInkscape(svgFileName, pngFileName, this.Width, this.Height);
		}

		#endregion
	}
}

