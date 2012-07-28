using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;

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
            ParsedPath svgFile = Target.InputFiles.Where(f => f.Extension == ".svg").First();
            ParsedPath pdfFile = Target.OutputFiles.Where(f => f.Extension == ".pdf").First();

			ConvertSvgToPdfWithInkscape(svgFile, pdfFile);
        }

        private bool ConvertSvgToPdfWithInkscape(string svgFile, string pdfFile)
        {
            Context.Output.Message(MessageImportance.Low, "Inkscape is converting {0} to {1}", svgFile, pdfFile);

            string output;
            string command = string.Format("\"{0}\" --export-pdf=\"{2}\" --file=\"{1}\"",
                ToolPaths.Inkscape, // 0
                svgFile, // 1
                pdfFile); // 2

            int ret = Command.Run(command, out output);

            if (ret != 0 || output.IndexOf("CRITICAL **") != -1)
            {
                throw new ContentFileException(Context.ContentFile, Target.LineNumber, String.Format("Error running Inkscape on '{0}'", svgFile));
            }

            return true;
        }

        #endregion
    }
}
