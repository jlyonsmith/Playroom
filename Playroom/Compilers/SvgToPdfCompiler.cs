﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playroom
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
        public BuildContext Context { get; set; }
        public BuildItem Item { get; set; }

        public void Compile()
        {
        }

        #endregion
    }
}