using System;
using System.Linq;
using ToolBelt;
using System.Collections.Generic;
using System.IO;
using TsonLibrary;

namespace Playroom
{
	public class CompilerExtension
	{
		private string input;
		private string output;

        public CompilerExtension(IEnumerable<ParsedPath> inputPaths, IEnumerable<ParsedPath> outputPaths)
        {
            Input = String.Join(PathUtility.PathSeparator, inputPaths.Select<ParsedPath, string>(p => p.Extension));
            Output = String.Join(PathUtility.PathSeparator, outputPaths.Select<ParsedPath, string>(p => p.Extension));
        }

        public CompilerExtension(IEnumerable<TsonStringNode> inputPaths, IEnumerable<TsonStringNode> outputPaths)
        {
            Input = String.Join(PathUtility.PathSeparator, inputPaths.Select<TsonStringNode, string>(n => Path.GetExtension(n.Value)));
            Output = String.Join(PathUtility.PathSeparator, outputPaths.Select<TsonStringNode, string>(n => Path.GetExtension(n.Value)));
        }

		public CompilerExtension(string input, string output)
		{
			Input = input;
			Output = output;
		}
		
		public string Input
		{ 
			get { return input; }
			set
			{
				input = NormalizeExtensionList(value);
			}
		}
		public string Output
		{ 
			get { return output; }
			set
			{
				output = NormalizeExtensionList(value);
			}
		}
		
		private string NormalizeExtensionList(string extensions)
		{
			return String.Join(PathUtility.PathSeparator, extensions
			                   .Split(new char[] { PathUtility.PathSeparatorChar, PathUtility.AltPathSeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
			                   .Select(s => (s.StartsWith(PathUtility.ExtensionSeparator) ? s : PathUtility.ExtensionSeparator + s).ToLower())
			                   .Distinct<string>()
			                   .OrderBy(i => i)); 
		}

		public override string ToString()
		{
			return string.Format("{0} -> {1}", input, output);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			
			return Equals(obj as CompilerExtension);
		}
		
		public bool Equals(CompilerExtension other)
		{
			if (other == null)
				return false;
			
			return String.CompareOrdinal(other.input, this.input) == 0 &&
				String.CompareOrdinal(other.output, this.output) == 0;
		}
		
		public override int GetHashCode()
		{
			return input.GetHashCode() ^ output.GetHashCode();
		}
	}
}

