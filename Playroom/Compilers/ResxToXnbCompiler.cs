using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playroom
{
    public class ResxToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".resx" }; }
        }

        public string[] OutputExtensions
        {
            get { return new string[] { ".xnb" }; }
        }

        public BuildContext Context { get; set; }
        public BuildItem Item { get; set; }

        public void Compile()
        {
        }

        #endregion
    }
}
