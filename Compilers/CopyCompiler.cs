using System;
using System.IO;
using ToolBelt;
using System.Linq;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace Playroom
{
	public class CopyCompiler : IContentCompiler 
	{
		#region IContentCompiler
		
		public CompilerExtension[] Extensions { get; private set; }
		public BuildContext Context { get; set; }
		public BuildTarget Target { get; set; }

		public void Setup(YamlMappingNode settings)
		{
			List<CompilerExtension> extensions = new List<CompilerExtension>();
			YamlSequenceNode sequence1;
			YamlMappingNode mapping1;
			YamlScalarNode scalar1, scalar2;
			
			settings.GetChildNode("extensions", out sequence1);
			
			foreach (var node in sequence1)
			{
				node.CastNode(out mapping1);
				mapping1.GetChildNode("inputs", out scalar1);
				mapping1.GetChildNode("outputs", out scalar2);
				
				extensions.Add(new CompilerExtension(scalar1.Value, scalar2.Value));
			}
			
			this.Extensions = extensions.ToArray();
		}

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

