using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playroom.Compilers
{
    public class SvgToPdfCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".svg" }; }
        }

        public string[] OutputExtensions
        {
            get { return new string[] { ".pdf" }; }
        }

        public void Compile(BuildContext buildContext, BuildItem buildItem)
        {
        }

        #endregion
    }
}
