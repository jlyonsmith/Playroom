using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Jamoki.Games.Spider
{
	public class TestRectangles
	{
		private Rectangle[] rectangles;

		public TestRectangles(Rectangle[] rectangles)
		{
			this.rectangles = rectangles;
		}

		public Rectangle Screen { get { return rect[0]; } }
		public Rectangle Circle1 { get { return rect[1]; } }
		public Rectangle Circle2 { get { return rect[2]; } }
	}

}
