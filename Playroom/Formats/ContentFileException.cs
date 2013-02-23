using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Playroom
{
    public class ContentFileException : Exception
    {
		public Mark Start { get; private set; }
		public Mark End { get; private set; }

		public ContentFileException(string message)
			: base(message)
		{
			Start = Mark.Empty;
			End = Mark.Empty;
		}
		
		public ContentFileException(Exception innerException)
			: base(innerException.Message, innerException)
		{
			Start = Mark.Empty;
			End = Mark.Empty;
		}
		
		public ContentFileException(string message, Exception innerException)
			: base(message, innerException)
		{
			Start = Mark.Empty;
			End = Mark.Empty;
		}

		public ContentFileException(YamlNode node) : base()
        {
			Start = node.Start;
			End = node.End;
		}

		public ContentFileException(YamlNode node, Exception innerException)
			: base(innerException.Message)
		{
			Start = node.Start;
			End = node.End;
		}
		
		public ContentFileException(YamlNode node, string message)
			: base(message)
		{
			Start = node.Start;
			End = node.End;
		}
		
		public ContentFileException(YamlNode node, string message, Exception innerException)
			: base(message, innerException)
		{
			Start = node.Start;
			End = node.End;
		}

		public ContentFileException(Mark start, string message)
			: base(message)
		{
			Start = End = start;
		}
		
		public ContentFileException(Mark start, string message, Exception innerException)
			: base(message, innerException)
		{
			Start = End = start;
		}

		// Helper function to strip line information off of YamlException
		public static string StripMessage(YamlException yamlException)
		{
			// Strip off the extra location information in the message
			string message = yamlException.Message;
			int n = message.IndexOf("): ");
			
			if (n != -1)
				message = message.Substring(n + 3);

			return message;
		}

		public ContentFileException(string message, YamlException innerException) : base(message)
		{
			Start = innerException.Start;
			End = innerException.End;
		}
	}
}
