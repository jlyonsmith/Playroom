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
            get { throw new NotImplementedException(); }
        }

        public string[] OutputExtensions
        {
            get { throw new NotImplementedException(); }
        }

        public BuildContext Context { get; set; }
        public BuildItem Item { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
