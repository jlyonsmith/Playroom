//
// This file was generated on 6/8/2012 12:06:18 PM.
//

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Company.Product
{
	public class LogoRectangles
	{
		private Rectangle[] rectangles;

		public LogoRectangles(Rectangle[] rectangles)
		{
			this.rectangles = rectangles;
		}

		public Rectangle Screen { get { return rectangles[0]; } }
		public Rectangle JamokiLogo { get { return rectangles[1]; } }
	}

	public class MainRectangles
	{
		private Rectangle[] rectangles;

		public MainRectangles(Rectangle[] rectangles)
		{
			this.rectangles = rectangles;
		}

		public Rectangle Screen { get { return rectangles[0]; } }
		public Rectangle ScrollIndicators { get { return rectangles[1]; } }
		public Rectangle SecondDrawPile { get { return rectangles[2]; } }
		public Rectangle Card { get { return rectangles[3]; } }
		public Rectangle GrownCard { get { return rectangles[4]; } }
		public Rectangle GrownCardSpacing { get { return rectangles[5]; } }
		public Rectangle DeckPile { get { return rectangles[6]; } }
		public Rectangle FirstDrawPile { get { return rectangles[7]; } }
		public Rectangle FirstPlayPile { get { return rectangles[8]; } }
		public Rectangle FirstPlayPile2ndCard { get { return rectangles[9]; } }
		public Rectangle SecondPlayPile { get { return rectangles[10]; } }
		public Rectangle SecondPlayPile2ndCard { get { return rectangles[11]; } }
		public Rectangle ThirdPlayPile { get { return rectangles[12]; } }
		public Rectangle ForthPlayPile { get { return rectangles[13]; } }
		public Rectangle FifthPlayPile { get { return rectangles[14]; } }
		public Rectangle SixthPlayPile { get { return rectangles[15]; } }
		public Rectangle SeventhPlayPile { get { return rectangles[16]; } }
		public Rectangle EighthPlayPile { get { return rectangles[17]; } }
		public Rectangle NinthPlayPile { get { return rectangles[18]; } }
		public Rectangle TenthPlayPile { get { return rectangles[19]; } }
		public Rectangle FirstDiscardPile { get { return rectangles[20]; } }
		public Rectangle Scoreboard { get { return rectangles[21]; } }
		public Rectangle SecondDiscardPile { get { return rectangles[22]; } }
		public Rectangle UndoButton { get { return rectangles[23]; } }
		public Rectangle NewButton { get { return rectangles[24]; } }
		public Rectangle HintButton { get { return rectangles[25]; } }
		public Rectangle MenuBackground { get { return rectangles[26]; } }
		public Rectangle MenuButton { get { return rectangles[27]; } }
		public Rectangle SettingsMenuButton { get { return rectangles[28]; } }
		public Rectangle HiddenMenuButton { get { return rectangles[29]; } }
		public Rectangle NewMenuButton { get { return rectangles[30]; } }
		public Rectangle ReplayMenuButton { get { return rectangles[31]; } }
		public Rectangle RoundHighlight { get { return rectangles[32]; } }
		public Rectangle SmallRoundHighlight { get { return rectangles[33]; } }
		public Rectangle Rectangle1 { get { return rectangles[34]; } }
		public Rectangle TimeTitle { get { return rectangles[35]; } }
		public Rectangle Time { get { return rectangles[36]; } }
		public Rectangle Rectangle2 { get { return rectangles[37]; } }
		public Rectangle Score { get { return rectangles[38]; } }
		public Rectangle ScoreTitle { get { return rectangles[39]; } }
		public Rectangle Moves { get { return rectangles[40]; } }
		public Rectangle MovesTitle { get { return rectangles[41]; } }
	}

}
