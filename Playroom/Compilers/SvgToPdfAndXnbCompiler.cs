using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.IO;

namespace Playroom
{
    public class SvgToPdfAndXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".svg" }; }
        }
        public string[] OutputExtensions
        {
            get { return new string[] { ".pdf", ".xnb" }; }
        }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
		{
			IEnumerable<ParsedPath> svgPaths = Target.InputFiles.Where(f => f.Extension == ".svg");
			IEnumerable<ParsedPath> pdfPaths = Target.OutputFiles.Where(f => f.Extension == ".pdf");
			ParsedPath xnbPath = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();

			int numSvgPaths = svgPaths.Count();
			int numPdfPaths = pdfPaths.Count();

			if (numSvgPaths != numPdfPaths)
				throw new ContentFileException("Number of SVG's ({0}) does not match number of PDF's ({1})".CultureFormat(numSvgPaths, numPdfPaths));

			for (int i = 0; i < numSvgPaths; i++)
			{
				ParsedPath pdfPath = pdfPaths.ElementAt(i);
				ParsedPath svgPath = svgPaths.ElementAt(i);

				if (!Directory.Exists(pdfPath.VolumeAndDirectory))
				{
					Directory.CreateDirectory(pdfPath.VolumeAndDirectory);
				}

				ImageTools.SvgToPdfWithInkscape(svgPath, pdfPath);
			}

			List<string> pdfInfo = new List<string>();

			pdfInfo.Add(this.Target.Properties.GetRequiredValue("Pinboard"));
			pdfInfo.Add(this.Target.Properties.GetRequiredValue("Rectangle"));

			int numRows;
			int numCols;

			this.Target.Properties.GetOptionalValue("Rows", out numRows, 1);
			this.Target.Properties.GetOptionalValue("Columns", out numCols, 1);

			if (numRows * numCols != numPdfPaths)
				throw new ContentFileException("Number of PDF's ({0}) does not match number of cells ({1})".CultureFormat(numPdfPaths, numRows * numCols));

			pdfInfo.Add(numRows.ToString());
			pdfInfo.Add(numCols.ToString());

			for (int row = 0; row < numRows; row++)
			{
				for (int col = 0; col < numCols; col++)
				{
					ParsedPath pdfPath = pdfPaths.ElementAt(row * numCols + col);

					pdfInfo.Add(pdfPath.File);
				}
			}

			if (!Directory.Exists(xnbPath.VolumeAndDirectory))
			{
				Directory.CreateDirectory(xnbPath.VolumeAndDirectory);
			}

			XnbFileWriterV5.WriteFile(pdfInfo, xnbPath);
		}

        #endregion
    }
}
