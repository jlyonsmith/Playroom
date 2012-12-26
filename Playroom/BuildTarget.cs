using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using ToolBelt;
using System.IO;

namespace Playroom
{
    public class BuildTarget
    {
		public BuildTarget(
			ContentFileV3.Target contentFileTarget, 
			FilePathGroup globalItems, 
			PropertyGroup globalProps, 
			IList<CompilerClass> compilerClasses)
		{
			this.Name = contentFileTarget.Name;
			this.Properties = new PropertyGroup(globalProps);
			this.Properties.Set("TargetName", this.Name);
			
			if (contentFileTarget.Properties != null)
				this.Properties.ExpandAndAddFromList(contentFileTarget.Properties, this.Properties);
			
			this.InputPaths = new ParsedPathList();

			string[] list = contentFileTarget.Inputs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			
			foreach (var rawInputFile in list)
			{
				ParsedPath pathSpec = null; 
				string s = Properties.ReplaceVariables(rawInputFile);
				
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
					
					this.InputPaths = new ParsedPathList(this.InputPaths.Concat(files));
				}
				else
				{
					if (!File.Exists(pathSpec))
					{
						throw new ContentFileException("Input file '{0}' does not exist".CultureFormat(pathSpec));
					}
					
					this.InputPaths.Add(pathSpec);
				}
			}
			
			this.OutputPaths = new ParsedPathList();
			
			list = contentFileTarget.Outputs.Split(';');
			
			foreach (var rawOutputFile in list)
			{
				string s = this.Properties.ReplaceVariables(rawOutputFile);
				
				try
				{
					ParsedPath outputFile = new ParsedPath(s, PathType.File).MakeFullPath();
					
					this.OutputPaths.Add(outputFile);
				}
				catch (Exception e)
				{
					throw new ContentFileException("Bad path '{0}'".CultureFormat(s), e);
				}
			}
			
			Func<IList<ParsedPath>, IEnumerable<string>> extractAndSortExtensions = (files) =>
			{
				return files.Select<ParsedPath, string>(f => f.Extension).Distinct<string>().OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase);
			};
			
			IEnumerable<string> inputExtensions = extractAndSortExtensions(this.InputPaths);
			IEnumerable<string> outputExtensions = extractAndSortExtensions(this.OutputPaths);
			
			CompilerClass compilerClass = null;
			
			if (String.IsNullOrEmpty(contentFileTarget.Compiler))
			{
				IEnumerator<CompilerClass> e = compilerClasses.GetEnumerator();
				
				while (e.MoveNext())
				{
					compilerClass = e.Current;
					
					if (inputExtensions.SequenceEqual(compilerClass.InputExtensions) &&
						outputExtensions.SequenceEqual(compilerClass.OutputExtensions))
					{
						break;
					}
				}
				
				if (compilerClass == null)
				{
					throw new ArgumentException(
						"No compiler found for target '{0}' handling extensions '{1}' -> '{2}'".CultureFormat(
		                Name, 
		                String.Join(Path.PathSeparator.ToString(), InputExtensions),
		                String.Join(Path.PathSeparator.ToString(), OutputExtensions)));
				}
			}
			else
			{
				// TODO: Search for the compiler based on the supplied name and validate it handles the extensions
				throw new ArgumentException("Supplied compiler '{0}' was not found".CultureFormat(contentFileTarget.Compiler));
			}
				
			SHA1 sha1 = SHA1.Create();
			StringBuilder sb = new StringBuilder();

			foreach (var property in contentFileTarget.Properties)
			{
				sb.Append(property.Item1);
				sb.Append(property.Item2);
			}

			sb.Append(contentFileTarget.Compiler);
			sb.Append(contentFileTarget.Inputs);
			sb.Append(contentFileTarget.Outputs);

			sha1.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
			Hash = BitConverter.ToString(sha1.Hash);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	
		public int LineNumber { get; private set; }
		public CompilerClass CompilerClass { get; private set; }
		public PropertyGroup Properties { get; private set; }
		public string Name { get; private set; }
		public IList<ParsedPath> InputPaths { get; private set; }
		public IEnumerable<string> InputExtensions { get; private set; }
		public IList<ParsedPath> OutputPaths { get; private set; }
		public IEnumerable<string> OutputExtensions { get; private set; }
		public string Hash { get; private set; }
    }
}
