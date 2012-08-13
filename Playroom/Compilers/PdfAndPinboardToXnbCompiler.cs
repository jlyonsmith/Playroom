using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.IO;

namespace Playroom
{
    public class PdfAndPinboardToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".pdf", ".pinboard" }; }
        }
        public string[] OutputExtensions
        {
            get { return new string[] { ".xnb" }; }
        }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
		{
			IEnumerable<ParsedPath> pdfFileNames = Target.InputFiles.Where(f => f.Extension == ".pdf");
			ParsedPath pinboardFileName = Target.InputFiles.Where(f => f.Extension == ".pinboard").First();
			ParsedPath xnbFileName = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();

			string rectangleName;

			if (!this.Context.Properties.TryGetValue("Rectangle", out rectangleName))
			{
				throw new ContentFileException("'Rectangle' property not present");
			}

			PinboardFileV1 pinboardFile = PinboardFileReaderV1.ReadFile(pinboardFileName);

			if (pinboardFile.GetRectangleInfoByName(rectangleName) == null)
				throw new ContentFileException("Rectangle '{0}' not present in pinboard file '{1}'".CultureFormat(
					rectangleName, pinboardFileName));

			List<byte[]> files = new List<byte[]>();

			foreach (var pdfFileName in pdfFileNames)
			{
				files.Add(File.ReadAllBytes(pdfFileName));
			}

			PdfContent pdfContent = new PdfContent(files, rectangleName);

			XnbFileWriterV5.WriteFile(pdfContent, xnbFileName);
        }

        #endregion
    }
}
