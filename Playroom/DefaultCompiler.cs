using System;
using System.IO;
using ToolBelt;
using System.Linq;
using System.Collections.Generic;

namespace Playroom
{
	public class DefaultCompiler 
	{
		public DefaultCompiler(string[] inputExtensions, string[] outputExtensions)
		{
			InputExtensions = inputExtensions;
			OutputExtensions = outputExtensions;
		}

		#region IContentCompiler
		
		public string[] InputExtensions { get; private set; }
		public string[] OutputExtensions { get; private set; } 
		public BuildContext Context { get; set; }
		public BuildTarget Target { get; set; }
		
		public void Compile()
		{
			IList<ParsedPath> fromPaths = Target.InputPaths;
			IList<ParsedPath> toPaths = Target.OutputPaths;

			List<FileStream> toStreams = new List<FileStream>();

			try
			{
				foreach (ParsedPath toPath in toPaths)
					toStreams.Add(new FileStream(toPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite));
				
				foreach (ParsedPath fromPath in fromPaths)
				{
					foreach (var toStream in toStreams)
					{
						byte[] fromData = File.ReadAllBytes(fromPath);
				
						toStream.Write(fromData, 0, fromData.Length);
					}
				}
			}
			finally
			{
				foreach (FileStream toStream in toStreams)
					toStream.Close();
			}
		}
		
		#endregion
	}
}

