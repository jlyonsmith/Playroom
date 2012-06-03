using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

namespace Playroom
{
    public class ContentFileException : Exception 
    {
        public ContentFileException(ParsedPath contentFile, int lineNumber) : base()
        {
            SetFileAndLineNumber(contentFile, lineNumber);
        }

        public ContentFileException(ParsedPath contentFile, int lineNumber, string message)
            : base(message)
        {
            SetFileAndLineNumber(contentFile, lineNumber);
        }

        public ContentFileException(ParsedPath contentFile, int lineNumber, string message, Exception innerException)
            : base(message, innerException)
        {
            SetFileAndLineNumber(contentFile, lineNumber);
        }

        private void SetFileAndLineNumber(ParsedPath file, int lineNumber)
        {
            this.Data["ContentFile"] = file;
            this.Data["LineNumber"] = lineNumber;
        }
    }
}
