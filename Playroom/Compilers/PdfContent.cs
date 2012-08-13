using System;
using System.Collections.Generic;

namespace Playroom
{
	public class PdfContent
	{
		public PdfContent(List<byte[]> files, string rectangleName)
		{
			this.Files = files;
			this.RectangleName = rectangleName;
		}

		public string RectangleName { get; private set; }
		public List<byte[]> Files { get; private set; }
	}
}

