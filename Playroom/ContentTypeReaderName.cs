using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Playroom
{
    public class ContentTypeReaderName
    {
        public ContentTypeReaderName()
        {
            // TODO: Parameters to set the fields...
        }

        public override string ToString()
        {
            // TODO: ...
            return base.ToString();
        }

        public string ClassName { get; set; }
        public string AssemblyName { get; set; }
        public Version Version { get; set; }
        public CultureInfo Culture { get; set; }
        public byte[] PublicKeyToken { get; set; }
        public int ReaderVersion { get; set; }
    }
}
