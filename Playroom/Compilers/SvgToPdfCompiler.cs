using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.IO;

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
        public BuildTarget Target { get; set; }

        public void Compile()
		{
			ParsedPath svgPath = Target.InputFiles.Where(f => f.Extension == ".svg").First();
			ParsedPath pdfPath = Target.OutputFiles.Where(f => f.Extension == ".pdf").First();

			if (!Directory.Exists(pdfPath.VolumeAndDirectory))
			{
				Directory.CreateDirectory(pdfPath.VolumeAndDirectory);
			}

			ImageTools.SvgToPdfWithInkscape(svgPath, pdfPath);
        }

        #endregion
    }
}
