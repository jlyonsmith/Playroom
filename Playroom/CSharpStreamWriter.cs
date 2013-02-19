using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ToolBelt;

namespace Playroom
{
	public class CSharpStreamWriter
	{
		private TextWriter writer;
		private Dictionary<string, string> args;
		private int indent;

		public int CurrentIndent { get { return indent; } }

		public CSharpStreamWriter(TextWriter writer) : this(writer, new Dictionary<string, string>())
		{
		}

		public CSharpStreamWriter(TextWriter writer, Dictionary<string, string> args)
		{
			this.writer = writer;
			this.args = args;
		}

		public void WriteLine(string format, params object[] parms)
		{
			if (format.Length == 1)
			{
				if (format == "}")
					Outdent();

				writer.Write(new String('\t', indent));

				if (format == "{")
					Indent();

				writer.Write(format);
				writer.Write('\n');

				return;
			}
			
			if (parms.Length > 0)
			{
				for (int i = 0; i < parms.Length; i++)
				{
					args[i.ToString()] = parms[i].ToString();
				}
			}

			writer.Write(new String('\t', indent));
			writer.Write(format.ReplaceTags("{{", "}}", args, TaggedStringOptions.LeaveUnknownTags));
			writer.Write('\n');
		}

		public void WriteLine()
		{
			writer.Write('\n');
		}

		public void Indent()
		{
			indent++;
		}

		public void Outdent()
		{
			if (indent > 0)
				indent--;
		}
	}
}

