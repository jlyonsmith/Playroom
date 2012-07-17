using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playroom
{
    public class WavToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".wav" }; }
        }

        public string[] OutputExtensions
        {
            get { return new string[] { ".xnb" }; }
        }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
        }

        #endregion
    }
}
