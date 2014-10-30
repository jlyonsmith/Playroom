using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using TsonLibrary;

namespace Playroom
{
    public class ContentFileException : Exception
    {
		public TextLocation Start { get; private set; }

		public ContentFileException(string message)
			: base(message)
		{
			Start = TextLocation.None;
		}
		
		public ContentFileException(Exception innerException)
			: base(innerException.Message, innerException)
		{
			Start = TextLocation.None;
		}
		
		public ContentFileException(string message, Exception innerException)
			: base(message, innerException)
		{
			Start = TextLocation.None;
		}

		public ContentFileException(TsonNode node) : base()
        {
			Start = node.Token.Location;
		}

		public ContentFileException(TsonNode node, Exception innerException)
			: base(innerException.Message)
		{
			Start = node.Token.Location;
		}
		
		public ContentFileException(TsonNode node, string message)
			: base(message)
		{
			Start = node.Token.Location;
		}
		
		public ContentFileException(TsonNode node, string message, Exception innerException)
			: base(message, innerException)
		{
			Start = node.Token.Location;
		}

		public ContentFileException(TextLocation location, string message)
			: base(message)
		{
			Start = location;
		}
		
		public ContentFileException(TextLocation location, string message, Exception innerException)
			: base(message, innerException)
		{
			Start = location;
		}

		public ContentFileException(string message, TsonParseException innerException) : base(message)
		{
			Start = innerException.ErrorLocation;
		}
	}
}
