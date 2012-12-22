using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using ToolBelt;
using Playroom.Formats;

namespace Playroom
{
    public class PinboardToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".pinboard" }; } }
        public string[] OutputExtensions { get { return new string[] { ".json" }; } }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
            ParsedPath pinboardPath = Target.InputFiles.Where(f => f.Extension == ".pinboard").First();
            ParsedPath jsonPath = Target.OutputFiles.Where(f => f.Extension == ".json").First();
            PinboardFileV1 pinboard = PinboardFileCache.Load(pinboardPath);
            Rectangle[] rectangles = new Rectangle[pinboard.RectInfos.Count + 1];

            rectangles[0] = new Rectangle(pinboard.ScreenRectInfo.X, pinboard.ScreenRectInfo.Y, pinboard.ScreenRectInfo.Width, pinboard.ScreenRectInfo.Height);

            for (int i = 0; i < pinboard.RectInfos.Count; i++)
            {
                rectangles[i + 1] = new Rectangle(pinboard.RectInfos[i].X, pinboard.RectInfos[i].Y, pinboard.RectInfos[i].Width, pinboard.RectInfos[i].Height);
            }

			if (!Directory.Exists(jsonPath.VolumeAndDirectory))
			{
				Directory.CreateDirectory(jsonPath.VolumeAndDirectory);
			}

            JsonFileWriter.WriteFile(jsonPath, rectangles);
        }

        #endregion
    }
}
