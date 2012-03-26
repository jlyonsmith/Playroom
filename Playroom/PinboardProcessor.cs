using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using ToolBelt;
using System.Xml;

namespace Playroom
{
    [ContentProcessor(DisplayName = "Pinboard Processor")]
    public class PinboardProcessor : ContentProcessor<PinboardData, Rectangle[]>
    {
        private ContentProcessorContext Context { get; set; }
        private ParsedPath PinboardFile { get; set; }

        public override Rectangle[] Process(PinboardData pinboardData, ContentProcessorContext context)
        {
            ParsedPath intermediateDir = new ParsedPath(context.IntermediateDirectory, PathType.Directory);

            this.Context = context;

            List<Rectangle> rectangles = new List<Rectangle>();

            System.Drawing.Rectangle rect = pinboardData.ScreenRectInfo.Rectangle;

            rectangles.Add(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));

            for (int i = 0; i < pinboardData.RectInfos.Count; i++)
            {
                rect = pinboardData.RectInfos[i].Rectangle;

                rectangles.Add(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
            }

            return rectangles.ToArray<Rectangle>();
        }
    }
}
