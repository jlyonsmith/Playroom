using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Cairo;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Playroom
{
    public class SpritefontAndStringsToXnbCompiler : IContentCompiler
    {
		#region Classes
		class CharacterData
		{
			// The character as a string.  The Cairo wrapper only supports strings
			public string Character { get; set; }
			// The bearing information from the Cairo TextExtent.  This is a point on the font baseline
			// from which Cairo draws the character.
			public Cairo.PointD Bearing { get; set; }
			// The location of the character in the generated bitmap.  The characters are currently
			// drawn in a horizontal strip with no padding and with their tops aligned.
			public Cairo.Rectangle Location { get; set; }
			// This is the XNA cropping information which has nothing to do with cropping (see Nuclex 
			// project SpriteFontContent.h for more details):
			//  - X/Y is the amount to move the characters bitmap from the "pen", which in the XNA
			//    drawing routines is at the upper left corner of the sprite.
			//  - W is the advancement after drawing the character
			//  - H is the line height of the font; the max ascent, max descent and inter line spacing
			public Cairo.Rectangle Cropping { get; set; }
			// This simple ABC spacing information, not real kerning information:
			//  - X is the space before the character bitmap
			//  - Y is the width of the character bitmap
			//  - Z is the space after the character bitmap
			public Vector3 Kerning { get; set; }
		}
		#endregion

        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".spritefont", ".strings" }; } }
        public string[] OutputExtensions { get { return new string[] { ".xnb" }; } }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
		{
			ParsedPath spriteFontFile = Target.InputFiles.Where(f => f.Extension == ".spritefont").First();
			ParsedPath stringsFile = Target.InputFiles.Where(f => f.Extension == ".strings").First();
			ParsedPath xnbFile = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();
        
			SpriteFontFile sff = SpriteFontFileReader.ReadFile(spriteFontFile);
			StringsFileV1 sf = StringsFileReaderV1.ReadFile(stringsFile);

			HashSet<char> hs = new HashSet<char>();

			foreach (var item in sf.Strings)
			{
				for (int i = 0; i < item.Value.Length; i++)
				{
					hs.Add(item.Value[i]);
				}
			}

			foreach (var region in sff.CharacterRegions)
			{
				for (char c = region.Start; c <= region.End; c++)
				{
					hs.Add(c);
				}
			}

			List<char> fontChars = hs.OrderBy(c => c).ToList();
			FontSlant fontSlant = (sff.Style == SpriteFontFile.FontStyle.Italic ? FontSlant.Italic : FontSlant.Normal);
			FontWeight fontWeight = (sff.Style == SpriteFontFile.FontStyle.Bold ? FontWeight.Bold : FontWeight.Normal);

			SpriteFontContent sfc = CreateSpriteFontContent(
				sff.FontName, sff.Size, fontSlant, fontWeight, sff.Spacing, sff.DefaultCharacter, fontChars);

			XnbFileWriterV5.WriteFile(sfc, xnbFile);

			if (this.Context.Properties.ContainsKey("WritePngFile"))
			{
				;
			}
		}

		static BitmapContent CreateBitmapContent(
			double bitmapWidth, 
			double bitmapHeight, 
			string fontName, 
			FontSlant fontSlant, 
			FontWeight fontWeight, 
			double fontSize, 
			List<CharacterData> cds)
		{
			using (ImageSurface surface = new ImageSurface(Format.Argb32, (int)bitmapWidth, (int)bitmapHeight))
			{
				using (Context g = new Context(surface))
				{
					g.SelectFontFace(fontName, fontSlant, fontWeight);
					g.SetFontSize(fontSize);
					g.Color = new Color(0, 0, 0);
					double x = 0;
					for (int i = 0; i < cds.Count; i++)
					{
						CharacterData cd = cds[i];
						g.MoveTo(x + cd.Bearing.X, cd.Bearing.Y);
						g.ShowText(cd.Character);
						x += cd.Location.Width;
					}
				}

				return new BitmapContent(SurfaceFormat.Color, surface.Width, surface.Height, surface.Data);
			}
		}

		static List<CharacterData> CreateCharacterData(
			string fontName, 
			FontSlant fontSlant, 
			FontWeight fontWeight, 
			double fontSize, 
			List<char> fontChars, 
			out double bitmapWidth, 
			out double bitmapHeight)
		{
			List<CharacterData> cds = new List<CharacterData>();

			using (ImageSurface surface = new ImageSurface(Format.Argb32, 256, 256))
			{
				using (Context g = new Context(surface))
				{
					g.SelectFontFace(fontName, fontSlant, fontWeight);
					g.SetFontSize(fontSize);

					FontExtents fe = g.FontExtents;
					double x = 0;
					double y = 0;

					for (int i = 0; i < fontChars.Count; i++)
					{
						CharacterData cd = new CharacterData();

						cds.Add(cd);
						cd.Character = new String(fontChars[i], 1);

						TextExtents te = g.TextExtents(cd.Character);

						cd.Bearing = new PointD(-te.XBearing, -te.YBearing);
						cd.Location = new Cairo.Rectangle(x, 0, te.Width, te.Height);
						cd.Cropping = new Cairo.Rectangle(0, fe.Ascent - cd.Bearing.Y, te.XAdvance, fe.Height);
						cd.Kerning = new Vector3(0, (float)cd.Location.Width, (float)(-te.XBearing + te.XAdvance - te.Width));

						x += te.Width;

						if (te.Height > y)
							y = te.Height;
					}

					bitmapWidth = x;
					bitmapHeight = y;
				}
			}

			return cds;
		}

		private SpriteFontContent CreateSpriteFontContent(
			string fontName, 
			double fontSize, 
			FontSlant fontSlant, 
			FontWeight fontWeight, 
			int spacing, 
			char? defaultChar, 
			List<char> fontChars)
		{
			double bitmapWidth = 0;
			double bitmapHeight = 0;
			List<CharacterData> cds = CreateCharacterData(
				fontName, fontSlant, fontWeight, fontSize, fontChars, out bitmapWidth, out bitmapHeight);

			BitmapContent bitmapContent = CreateBitmapContent(
				bitmapWidth, bitmapHeight, fontName, fontSlant, fontWeight, fontSize, cds);

			List<Microsoft.Xna.Framework.Rectangle> locations = new List<Microsoft.Xna.Framework.Rectangle>();
			List<Microsoft.Xna.Framework.Rectangle> croppings = new List<Microsoft.Xna.Framework.Rectangle>();
			List<Vector3> kernings = new List<Vector3>();

			foreach (var cd in cds)
			{
				locations.Add(new Microsoft.Xna.Framework.Rectangle(
					(int)cd.Location.X, (int)cd.Location.Y, (int)cd.Location.Width, (int)cd.Location.Height));
				croppings.Add(new Microsoft.Xna.Framework.Rectangle(
					(int)cd.Cropping.X, (int)cd.Cropping.Y, (int)cd.Location.Width, (int)cd.Location.Height));
				kernings.Add(cd.Kerning);
			}

			int verticalSpacing = 0;
			float horizontalSpacing = 0;

			return new SpriteFontContent(
				new Texture2DContent(bitmapContent), 
				locations, 
				fontChars, 
				croppings, 
				verticalSpacing, 
				horizontalSpacing, 
				kernings, 
				defaultChar);
		}

        #endregion
    }
}
