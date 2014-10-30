using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using ToolBelt;
using System.IO;
using TsonLibrary;

namespace Playroom
{
    public sealed class BuildTarget
    {
		public CompilerClass CompilerClass { get; private set; }
		public string Name { get; private set; }
		public IList<ParsedPath> InputPaths { get; private set; }
		public IList<ParsedPath> OutputPaths { get; private set; }
		public CompilerExtension Extension { get; private set; }
		public string Hash { get; private set; }
		public PropertyCollection Properties { get; private set; }
		public ContentFileV4.Target RawTarget{ get; set; }

		public BuildTarget(
			ContentFileV4.Target rawTarget, 
			BuildContext buildContext)
		{
            this.RawTarget = rawTarget;
            this.Name = RawTarget.Name.Value;

			if (RawTarget.Inputs.Count == 0)
				throw new ContentFileException(RawTarget.Name, "Target must have at least one input");

			this.Properties = new PropertyCollection();
			this.Properties.Set("TargetName", this.Name);

			List<ParsedPath> inputPaths = new List<ParsedPath>();

			foreach (var rawInputFile in RawTarget.Inputs)
			{
				ParsedPath pathSpec = null; 
				string s;
				
				try
				{
					s = buildContext.Properties.ExpandVariables(rawInputFile.Value, false);
					s = this.Properties.ExpandVariables(s);
				}
				catch (Exception e)
				{
					throw new ContentFileException(rawInputFile, e);
				}

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
			
			if (RawTarget.Outputs.Count == 0)
				throw new ContentFileException(RawTarget.Name, "Target must have at least one output");

			foreach (var rawOutputFile in RawTarget.Outputs)
			{
				string s;

				try
				{
					s = buildContext.Properties.ExpandVariables(rawOutputFile.Value, false);
					s = this.Properties.ExpandVariables(s);
				}
				catch (Exception e)
				{
					throw new ContentFileException(rawOutputFile, e);
				}
				
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

			if (RawTarget.Compiler == null || RawTarget.Compiler.Value.Length == 0)
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
					if (compilerClass.Name.EndsWith(RawTarget.Compiler.Value, StringComparison.OrdinalIgnoreCase))
					{
						this.CompilerClass = compilerClass;
						break;
					}
				}

				if (this.CompilerClass == null)
					throw new ArgumentException("Supplied compiler '{0}' was not found".CultureFormat(RawTarget.Compiler));
			}

			SHA1 sha1 = SHA1.Create();
			StringBuilder sb = new StringBuilder();

			sb.Append(RawTarget.Inputs);
			sb.Append(RawTarget.Outputs);
			
			if (RawTarget.Compiler != null)
				sb.Append(RawTarget.Compiler);

			if (RawTarget.Parameters != null)
			{
				foreach (var parameter in RawTarget.Parameters.KeyValues)
				{
					sb.Append(parameter.Key.Value);
                    sb.Append(parameter.Value.ToString());
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
