using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playroom
{
    public class PdfAndPinboardToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".svg", ".pinboard" }; }
        }

        public string[] OutputExtensions
        {
            get { return new string[] { ".pdf" }; }
        }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
        }

        #endregion
    }
}
