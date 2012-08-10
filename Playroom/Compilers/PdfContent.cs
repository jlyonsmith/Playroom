using System;
using System.Collections.Generic;

namespace Playroom
{
	public class PdfContent
	{
		public PdfContent(IList<byte[]> files, string rectangleName)
		{
		}

		public string RectangleName { get; private set; }
		public List<byte[]> Files { get; private set; }
	}
}

