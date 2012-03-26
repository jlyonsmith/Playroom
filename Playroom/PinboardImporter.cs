using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using ToolBelt;
using System.IO;
using System.Xml;

namespace Playroom
{
    [ContentImporter(".pinboard", DisplayName = "Pinboard Importer", DefaultProcessor = "PinboardProcessor")]
    public class PinboardImporter : ContentImporter<PinboardData>
    {
        public override PinboardData Import(string fileName, ContentImporterContext context)
        {
            ParsedPath pinboardFile = new ParsedPath(fileName, PathType.File);

            if (!File.Exists(pinboardFile))
            {
                throw new FileNotFoundException(PlayroomResources.FileNotFound(pinboardFile));
            }

            PinboardData pinboardData = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(pinboardFile))
                {
                    pinboardData = PinboardDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception e)
            {
                throw new InvalidContentException(
                    String.Format("Unable to read pinboard data. {0}", e.Message), new ContentIdentity(pinboardFile), e);
            }

            return pinboardData;
        }
    }
}
