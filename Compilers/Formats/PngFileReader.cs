using System;
using Cairo;
using ToolBelt;

namespace Playroom
{
	public class PngFileReader
	{
		public PngFileReader()
		{
		}

		public static PngFile ReadFile(ParsedPath pngFileName)
		{
			PngFile pngFile = new PngFile();

			ImageSurface image = new ImageSurface(pngFileName);

			if (image.Format != Format.ARGB32)
				throw new NotSupportedException("Only PNG's in ARGB32 format currently supported");

			pngFile.Width = image.Width;
			pngFile.Height = image.Height;
			pngFile.RgbaData = GetRgbaData(image);

			return pngFile;
		}

        private unsafe static byte[] GetRgbaData(ImageSurface image)
        {
            byte[] rgba = new byte[4 * image.Width * image.Height];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        rgba[(y * image.Width + x) * 4 + i] = ((byte*)image.DataPtr)[y * image.Stride + x * 4 + i];
                    }
                }
            }

            return rgba;
        }
	}
}

