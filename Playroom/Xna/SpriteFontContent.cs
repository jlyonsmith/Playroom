using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Playroom;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
	public class SpriteFontContent
	{
		public Texture2DContent Texture { get; set; }
		public IList<Rectangle> Glyphs { get; set; }
		public IList<char> CharacterMap { get; set; }
		public List<Rectangle> Cropping { get; set; }
		public int VerticalSpacing { get; set; }
		public int HorizontalSpacing { get; set; }
		public IList<Vector3> Kerning { get; set; }
		public char? DefaultCharacter { get; set; }
	}
}

