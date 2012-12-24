using System;
using Playroom;
using System.IO;
using ToolBelt;
using System.Linq;

namespace Playroom.Compilers
{
	public class PngToPngCompiler : IContentCompiler
	{
		#region IContentCompiler

		public string[] InputExtensions { get { return new string[] { ".png" }; } }

		public string[] OutputExtensions { get { return new string[] { ".png" }; } }

		public BuildContext Context { get; set; }

		public BuildTarget Target { get; set; }

		public void Compile()
		{
			ParsedPath pngFromPath = Target.InputPaths.Where(f => f.Extension == ".png").First();
			ParsedPath pngToPath = Target.OutputPaths.Where(f => f.Extension == ".png").First();

			File.Copy(pngFromPath, pngToPath, true);
		}
		
		#endregion
	}
}

