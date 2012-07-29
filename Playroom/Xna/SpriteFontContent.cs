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
		public SpriteFontContent()
		{
		}

		public Texture2DContent Texture { get; private set; }
		public IList<Rectangle> Glyphs { get; private set; }
		public IList<char> CharacterMap { get; private set; }
		public List<Rectangle> Cropping { get; private set; }
		public int VerticalSpacing { get; private set; }
		public int HorizontalSpacing { get; private set; }
		public IList<Vector3> Kerning { get; private set; }
		public char? DefaultCharacter { get; private set; }
	}
}

