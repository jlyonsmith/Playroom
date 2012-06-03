using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Company.Product
{
	public class AboutRectangles
	{
		private Rectangle[] rectangles;

		public AboutRectangles(Rectangle[] rectangles)
		{
			this.rectangles = rectangles;
		}

		public Rectangle Screen { get { return rectangles[0]; } }
		public Rectangle AboutContent { get { return rectangles[1]; } }
		public Rectangle AboutLabel { get { return rectangles[2]; } }
		public Rectangle BackButton { get { return rectangles[3]; } }
		public Rectangle JamokiUrl { get { return rectangles[4]; } }
	}

	public class HelpRectangles
	{
		private Rectangle[] rectangles;

		public HelpRectangles(Rectangle[] rectangles)
		{
			this.rectangles = rectangles;
		}

		public Rectangle Screen { get { return rectangles[0]; } }
		public Rectangle HelpLabel { get { return rectangles[1]; } }
		public Rectangle BackButton { get { return rectangles[2]; } }
		public Rectangle HelpContent { get { return rectangles[3]; } }
	}

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

	public class SettingsRectangles
	{
		private Rectangle[] rectangles;

		public SettingsRectangles(Rectangle[] rectangles)
		{
			this.rectangles = rectangles;
		}

		public Rectangle Screen { get { return rectangles[0]; } }
		public Rectangle LevelLabel { get { return rectangles[1]; } }
		public Rectangle HintOnOffLabel { get { return rectangles[2]; } }
		public Rectangle AudioOnOffLabel { get { return rectangles[3]; } }
		public Rectangle BeginnerLabel { get { return rectangles[4]; } }
		public Rectangle TimeOnOffLabel { get { return rectangles[5]; } }
		public Rectangle AdvancedLabel { get { return rectangles[6]; } }
		public Rectangle IntermediateLabel { get { return rectangles[7]; } }
		public Rectangle HintOnOffButton { get { return rectangles[8]; } }
		public Rectangle AudioOnOffButton { get { return rectangles[9]; } }
		public Rectangle TimeOnOffButton { get { return rectangles[10]; } }
		public Rectangle AboutButton { get { return rectangles[11]; } }
		public Rectangle StatisticsButton { get { return rectangles[12]; } }
		public Rectangle RectangularHighlight { get { return rectangles[13]; } }
		public Rectangle FeedbackButton { get { return rectangles[14]; } }
		public Rectangle HelpButton { get { return rectangles[15]; } }
		public Rectangle BackButton { get { return rectangles[16]; } }
		public Rectangle SettingsLabel { get { return rectangles[17]; } }
		public Rectangle BeginnerButton { get { return rectangles[18]; } }
		public Rectangle HighlightButton { get { return rectangles[19]; } }
		public Rectangle IntermediateButton { get { return rectangles[20]; } }
		public Rectangle AdvancedButton { get { return rectangles[21]; } }
	}

	public class SpiderRectangles
	{
		private Rectangle[] rectangles;

		public SpiderRectangles(Rectangle[] rectangles)
		{
			this.rectangles = rectangles;
		}

		public Rectangle Screen { get { return rectangles[0]; } }
		public Rectangle SpiderLogo { get { return rectangles[1]; } }
	}

	public class StatisticsRectangles
	{
		private Rectangle[] rectangles;

		public StatisticsRectangles(Rectangle[] rectangles)
		{
			this.rectangles = rectangles;
		}

		public Rectangle Screen { get { return rectangles[0]; } }
		public Rectangle IntermediateButton { get { return rectangles[1]; } }
		public Rectangle BeginnerButton { get { return rectangles[2]; } }
		public Rectangle HighlightButton { get { return rectangles[3]; } }
		public Rectangle AdvancedButton { get { return rectangles[4]; } }
		public Rectangle ResetButton { get { return rectangles[5]; } }
		public Rectangle StatisticsLabel { get { return rectangles[6]; } }
		public Rectangle GamesWonLabel { get { return rectangles[7]; } }
		public Rectangle GamesPlayedLabel { get { return rectangles[8]; } }
		public Rectangle FastestTimeLabel { get { return rectangles[9]; } }
		public Rectangle CurrentStreakLabel { get { return rectangles[10]; } }
		public Rectangle LongestStreakLabel { get { return rectangles[11]; } }
		public Rectangle HighestScoreLabel { get { return rectangles[12]; } }
		public Rectangle HighestScoreNo { get { return rectangles[13]; } }
		public Rectangle GamesWonNo { get { return rectangles[14]; } }
		public Rectangle GamesPlayedNo { get { return rectangles[15]; } }
		public Rectangle FastestTimeNo { get { return rectangles[16]; } }
		public Rectangle CurrentStreakNo { get { return rectangles[17]; } }
		public Rectangle LongestStreakNo { get { return rectangles[18]; } }
		public Rectangle BackButton { get { return rectangles[19]; } }
		public Rectangle BeginnerLabel { get { return rectangles[20]; } }
		public Rectangle IntermediateLabel { get { return rectangles[21]; } }
		public Rectangle AdvancedLabel { get { return rectangles[22]; } }
	}

}
