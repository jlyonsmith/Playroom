using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using ToolBelt;
using System.IO;
using YamlDotNet.Core;

namespace Playroom
{
    public sealed class BuildTarget
    {
		public Mark Start { get; private set; }
		public CompilerClass CompilerClass { get; private set; }
		public PropertyCollection Properties { get; private set; }
		public string Name { get; private set; }
		public IList<ParsedPath> InputPaths { get; private set; }
		public IList<ParsedPath> OutputPaths { get; private set; }
		public CompilerExtension Extension { get; private set; }
		public string Hash { get; private set; }

		public BuildTarget(
			ContentFileV4.Target contentFileTarget, 
			BuildContext buildContext)
		{
			this.Name = contentFileTarget.Name.Value;
			this.Start = contentFileTarget.Name.Start;
			this.Properties = new PropertyCollection(buildContext.Properties);
			this.Properties.Set("TargetName", this.Name);
			
			if (contentFileTarget.Properties != null)
				this.Properties.AddFromList(
					contentFileTarget.Properties.Select(p => new KeyValuePair<string, string>(p.Name.Value, p.Value.Value)));
			
			if (contentFileTarget.Inputs.Count == 0)
				throw new ContentFileException(contentFileTarget.Name, "Target must have at least one input");

			List<ParsedPath> inputPaths = new List<ParsedPath>();
			IEnumerable<string> list = contentFileTarget.Inputs.Select(p => p.Value);
			
			foreach (var rawInputFile in list)
			{
				ParsedPath pathSpec = null; 
				string s = Properties.ExpandVariables(rawInputFile);
				
				try
				{
					pathSpec = new ParsedPath(s, PathType.File).MakeFullPath();
				}
				catch (Exception e)
				{
					throw new ContentFileException("Bad path '{0}'".CultureFormat(s), e);
				}
				
				if (pathSpec.HasWildcards)
				{
					if (!Directory.Exists(pathSpec.VolumeAndDirectory))
					{
						throw new ContentFileException("Directory '{0}' does not exist".CultureFormat(pathSpec.VolumeAndDirectory));
					}
					
					IList<ParsedPath> files = DirectoryUtility.GetFiles(pathSpec, SearchScope.DirectoryOnly);
					
					if (files.Count == 0)
					{
						throw new ContentFileException("Wildcard input refers to no files after expansion");
					}
					
					inputPaths = new List<ParsedPath>(inputPaths.Concat(files));
				}
				else
				{
					inputPaths.Add(pathSpec);
				}
			}

			inputPaths.Sort();
			this.InputPaths = inputPaths;
			
			List<ParsedPath> outputPaths = new List<ParsedPath>();
			
			if (contentFileTarget.Outputs.Count == 0)
				throw new ContentFileException(contentFileTarget.Name, "Target must have at least one output");

			list = contentFileTarget.Outputs.Select(p => p.Value);
			
			foreach (var rawOutputFile in list)
			{
				string s = this.Properties.ExpandVariables(rawOutputFile);
				
				try
				{
					ParsedPath outputFile = new ParsedPath(s, PathType.File).MakeFullPath();
					
					outputPaths.Add(outputFile);
				}
				catch (Exception e)
				{
					throw new ContentFileException("Bad path '{0}'".CultureFormat(s), e);
				}
			}

			outputPaths.Sort();
			this.OutputPaths = outputPaths;

			this.Extension = new CompilerExtension(this.InputPaths.AsEnumerable(), this.OutputPaths.AsEnumerable());

			if (contentFileTarget.Compiler == null || contentFileTarget.Compiler.Value.Length == 0)
			{
				IEnumerator<CompilerClass> e = buildContext.CompilerClasses.GetEnumerator();
				
				while (e.MoveNext())
				{
					foreach (CompilerExtension extension in e.Current.Extensions)
					{
						if (extension.Equals(this.Extension))
						{
							this.CompilerClass = e.Current;
							break;
						}
					}

					if (this.CompilerClass != null)
						break;
				}
				
				if (this.CompilerClass == null)
				{
					throw new ArgumentException(
						"No compiler found for target '{0}' handling extensions '{1}'".CultureFormat(this.Name, this.Extension.ToString()));
				}
			}
			else
			{
				// Search for the compiler based on the supplied name and validate it handles the extensions
				foreach (var compilerClass in buildContext.CompilerClasses)
				{
					if (compilerClass.Name.EndsWith(contentFileTarget.Compiler.Value, StringComparison.OrdinalIgnoreCase))
					{
						this.CompilerClass = compilerClass;
						break;
					}
				}

				if (this.CompilerClass == null)
					throw new ArgumentException("Supplied compiler '{0}' was not found".CultureFormat(contentFileTarget.Compiler));
			}

			SHA1 sha1 = SHA1.Create();
			StringBuilder sb = new StringBuilder();

			sb.Append(contentFileTarget.Inputs);
			sb.Append(contentFileTarget.Outputs);
			
			if (contentFileTarget.Compiler != null)
				sb.Append(contentFileTarget.Compiler);

			if (contentFileTarget.Properties != null)
			{
				foreach (var property in contentFileTarget.Properties)
				{
					sb.Append(property.Name);
					sb.Append(property.Value);
				}
			}

			sha1.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
			Hash = BitConverter.ToString(sha1.Hash).Replace("-", "");
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
    }
}
