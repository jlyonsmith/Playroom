//
// This file was generated on 2/6/2013 10:39:44 AM.
//

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace 
{
	public class ExampleRectangles
	{
		private Rectangle[] rectangles;

		public ExampleRectangles(Rectangle[] rectangles)
		{
			this.rectangles = rectangles;
		}

		public Rectangle Screen { get { return rectangles[0]; } }
		public Rectangle Thing { get { return rectangles[1]; } }
	}

	public static class Rectangles
	{
		public static ExampleRectangles Example { get; set; }
	}
}
