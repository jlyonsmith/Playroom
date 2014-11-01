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
		public TextLocation ErrorLocation { get; private set; }

		public ContentFileException(string message)
			: base(message)
		{
			ErrorLocation = TextLocation.None;
		}
		
		public ContentFileException(Exception innerException)
			: base(innerException.Message, innerException)
		{
			ErrorLocation = TextLocation.None;
		}
		
		public ContentFileException(string message, Exception innerException)
			: base(message, innerException)
		{
			ErrorLocation = TextLocation.None;
		}

		public ContentFileException(TsonNode node) : base()
        {
			ErrorLocation = node.Token.Location;
		}

        public ContentFileException(TsonNode node, string message)
            : base(message)
        {
            ErrorLocation = node.Token.Location;
        }

        public ContentFileException(TsonNode node, Exception innerException)
            : base("", innerException)
        {
            ErrorLocation = node.Token.Location;
        }

		public ContentFileException(TsonNode node, string message, Exception innerException)
			: base(message, innerException)
		{
			ErrorLocation = node.Token.Location;
		}

		public ContentFileException(TextLocation location, string message)
			: base(message)
		{
			ErrorLocation = location;
		}
		
		public ContentFileException(TextLocation location, string message, Exception innerException)
			: base(message, innerException)
		{
			ErrorLocation = location;
		}
	}
}
