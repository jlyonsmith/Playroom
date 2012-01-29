using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;

using TInput = Playroom.PrismData;
using System.Xml;

namespace Playroom
{
    [ContentImporter(".prism", DisplayName = "Prism Data")]
    public class PrismDataImporter : ContentImporter<TInput>
    {
        public override TInput Import(string fileName, ContentImporterContext context)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Cannot read prism data '{0}'.  The file could not be found", fileName); 
            }

            PrismData prismData = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(fileName))
                {
                    prismData = PrismDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception e)
            {
                throw new InvalidContentException("Unable to read prism data", new ContentIdentity(fileName), e);
            }

            // TODO-john-2012: Create PngFile names in intermediate directory

            return prismData;
        }
    }
}
