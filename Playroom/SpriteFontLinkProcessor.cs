using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;

namespace Playroom
{
    [ContentProcessor(DisplayName = "Linked Sprite Font Description Processor")]
    public class SpriteFontLinkProcessor : ContentProcessor<LinkData, FontDescription>
    {
        public override FontDescription Process(LinkData input, ContentProcessorContext context)
        {
            return context.BuildAndLoadAsset<FontDescription, FontDescription>(
                new ExternalReference<FontDescription>(input.LinkedAssetFile), null, null, null);
        }
    }
}