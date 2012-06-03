using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;
using Microsoft.Xna.Framework;

namespace Playroom
{
    public class PinboardToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".pinboard" }; } }
        public string[] OutputExtensions { get { return new string[] { ".xnb" }; } }
        public BuildContext Context { get; set; }
        public BuildItem Item { get; set; }

        public void Compile()
        {
            ParsedPath pinboardFile = Item.InputFiles.Where(f => f.Extension == ".pinboard").First();
            ParsedPath xnbFile = Item.OutputFiles.Where(f => f.Extension == ".xnb").First();

            PinboardFileV1 pinboard = PinboardFileReaderV1.ReadFile(pinboardFile);

            Rectangle[] rectangles = new Rectangle[pinboard.RectInfos.Count + 1];

            rectangles[0] = new Rectangle(pinboard.ScreenRectInfo.X, pinboard.ScreenRectInfo.Y, pinboard.ScreenRectInfo.Width, pinboard.ScreenRectInfo.Height);

            for (int i = 0; i < pinboard.RectInfos.Count; i++)
            {
                rectangles[i + 1] = new Rectangle(pinboard.RectInfos[i].X, pinboard.RectInfos[i].Y, pinboard.RectInfos[i].Width, pinboard.RectInfos[i].Height);
            }

            List<ContentTypeWriter> typeWriters = new List<ContentTypeWriter>();

            // TODO: 

            XnbFileWriterV5.WriteFile(rectangles, typeWriters, xnbFile);
        }

        #endregion
    }
}
