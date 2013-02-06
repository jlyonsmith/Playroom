using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Json;
using ToolBelt;
using YamlDotNet.RepresentationModel.Serialization;

namespace Playroom.Compilers
{
    public class PinboardToJsonCompiler : IContentCompiler
    {
		#region Fields
		private CompilerExtension[] extensions = new CompilerExtension[]
		{
			new CompilerExtension(".pinboard", ".json")
		};
		#endregion 
		
		#region IContentCompiler
		public IList<CompilerExtension> Extensions { get { return extensions; } }
		public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

		public void Compile()
		{
			if (Target.InputPaths.Count != 1)
				throw new ContentFileException(Target.Start, "One input file expected");
			
			if (Target.OutputPaths.Count != 1)
				throw new ContentFileException(Target.Start, "One output file expected");
			
			ParsedPath pinboardPath = Target.InputPaths[0];
			ParsedPath jsonPath = Target.OutputPaths[0];
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

			var serializer = new Serializer();

			using (StreamWriter writer = new StreamWriter(jsonPath))
			{
				serializer.Serialize(writer, rectangles, SerializationOptions.JsonCompatible);
			}
        }

        #endregion
    }
}
