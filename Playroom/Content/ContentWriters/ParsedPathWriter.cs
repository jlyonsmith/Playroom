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
    [ContentTypeWriter]
    public class ParsedPathWriter : ContentTypeWriter<ParsedPath>
    {
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(ParsedPathReader).AssemblyQualifiedName;
        }
        
        protected override void Write(ContentWriter output, ParsedPath value)
        {
            output.Write(value.ToString());
            output.Write((short)(value.IsVolume ? PathType.Volume : value.IsDirectory ? PathType.Directory : PathType.File));
        }
    }

    public class ParsedPathReader : ContentTypeReader<ParsedPath>
    {
        protected override ParsedPath Read(ContentReader input, ParsedPath existingInstance)
        {
            return new ParsedPath(input.ReadString(), (PathType)input.ReadUInt16());
        }
    }
}
