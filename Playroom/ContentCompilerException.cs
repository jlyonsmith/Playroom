using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playroom
{
    public class ContentCompilerException : Exception 
    {
        public ContentCompilerException() : base()
        {
        }

        public ContentCompilerException(string message) : base(message)
        {
        }
    }
}
