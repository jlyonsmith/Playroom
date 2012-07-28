using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ToolBelt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;

namespace Playroom
{
    public class SpritefontAndStringsToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions { get { return new string[] { ".spritefont", ".strings" }; } }
        public string[] OutputExtensions { get { return new string[] { ".xnb" }; } }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
            ParsedPath spritefontFile = Target.InputFiles.Where(f => f.Extension == ".spritefont").First();
            ParsedPath stringsFile = Target.InputFiles.Where(f => f.Extension == ".strings").First();
            ParsedPath xnbFile = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();
        
			SpriteFontFile sff = SpriteFontFileReader.ReadFile(spritefontFile);
			StringsFileV1 strs = StringsFileReaderV1.ReadFile(stringsFile);

			string extraChars = new String(
				strs.Strings.Aggregate(new StringBuilder(), (sb, item) => sb.Append(item.Value)).ToString().Distinct().ToArray());

			SpriteFontContent sfc = CreateSpriteFontContent(sff, extraChars);

            XnbFileWriterV5.WriteFile(sfc, xnbFile);
		}

		private SpriteFontContent CreateSpriteFontContent(SpriteFontFile sff, string extraChars)
		{
			SpriteFontContent sfc = new SpriteFontContent();

			// TODO: Create the content

			return sfc;
		}

        #endregion
    }
}
