using System;
using Playroom;
using System.IO;
using ToolBelt;
using System.Linq;

namespace Playroom.Compilers
{
	public class WavToWavCompiler : IContentCompiler
	{
		#region IContentCompiler
		
		public string[] InputExtensions { get { return new string[] { ".wav" }; } }
		
		public string[] OutputExtensions { get { return new string[] { ".wav" }; } }
		
		public BuildContext Context { get; set; }
		
		public BuildTarget Target { get; set; }
		
		public void Compile()
		{
			ParsedPath wavFromPath = Target.InputPaths.Where(f => f.Extension == ".wav").First();
			ParsedPath wavToPath = Target.OutputPaths.Where(f => f.Extension == ".wav").First();
			
			File.Copy(wavFromPath, wavToPath, true);
		}
		
		#endregion
	}
}

