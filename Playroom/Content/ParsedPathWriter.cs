using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ToolBelt;
using Microsoft.Xna.Framework.Content;

namespace Playroom
{
    public class ParsedPathWriter : ContentTypeWriter<ParsedPath>
    {
        public override void Write(ContentWriter writer, ParsedPath value)
        {
            writer.Write(value.ToString());
            writer.Write((short)(value.IsVolume ? PathType.Volume : value.IsDirectory ? PathType.Directory : PathType.File));
        }

        public override ContentTypeReaderName GetReaderName()
        {
            return new ContentTypeReaderName();
        }
    }

    public class ParsedPathReader : ContentTypeReader<ParsedPath>
    {
        public override void Read(ContentReader reader, out ParsedPath value)
        {
            value = new ParsedPath(reader.ReadString(), (PathType)reader.ReadUInt16());
        }
    }
}
