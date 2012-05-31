using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;

namespace Playroom.Converters
{
    public class PinboardToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".pinboard" }; } }

        public string[] OutputExtensions { get { return new string[] { ".xnb" }; } }

        public void Compile(BuildContext buildContext, BuildItem buildItem)
        {
        }

        #endregion
    }
}
