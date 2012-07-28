using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace Playroom
{
    public class WavToXnbCompiler : IContentCompiler
    {
        #region IContentCompiler Members

        public string[] InputExtensions
        {
            get { return new string[] { ".wav" }; }
        }

        public string[] OutputExtensions
        {
            get { return new string[] { ".xnb" }; }
        }
        public BuildContext Context { get; set; }
        public BuildTarget Target { get; set; }

        public void Compile()
        {
            ParsedPath wavFile = Target.InputFiles.Where(f => f.Extension == ".wav").First();
            ParsedPath xnbFile = Target.OutputFiles.Where(f => f.Extension == ".xnb").First();

			AudioContent ac = AudioContent.FromFile(wavFile);

            XnbFileWriterV5.WriteFile(ac, xnbFile);
        }

        #endregion
    }
}
