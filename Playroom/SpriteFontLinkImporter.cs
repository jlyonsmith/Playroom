using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;
using System.Xml;
using ToolBelt;

namespace Playroom
{
    [ContentImporter(".spritefontlink", DisplayName = "Linked Sprite Font Description Importer", DefaultProcessor = "SpriteFontLinkProcessor")]
    public class SpriteFontLinkImporter : ContentImporter<LinkData>
    {
        public override LinkData Import(string fileName, ContentImporterContext context)
        {
            ParsedPath linkFile = new ParsedPath(fileName, PathType.File);

            if (!File.Exists(linkFile))
            {
                throw new FileNotFoundException("Cannot read link data '{0}'.  The file could not be found", linkFile);
            }

            LinkData linkData = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(linkFile))
                {
                    linkData = LinkDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception e)
            {
                throw new InvalidContentException("Unable to read link data", new ContentIdentity(fileName), e);
            }

            linkData.AssetFile = linkFile.File.MakeFullPath(linkFile);
            linkData.LinkedAssetFile = linkData.LinkedAssetFile.MakeFullPath(linkFile);

            return linkData;
        }
    }
}
