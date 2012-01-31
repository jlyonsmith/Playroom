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
using ToolBelt;
using System.Xml;

namespace Playroom
{
    [ContentImporter(".prism", DisplayName = "Prism Importer", DefaultProcessor = "PrismProcessor")]
    public class PrismImporter : ContentImporter<PrismData>
    {
        public override PrismData Import(string fileName, ContentImporterContext context)
        {
            ParsedPath prismFile = new ParsedPath(fileName, PathType.File);

            if (!File.Exists(prismFile))
            {
                throw new FileNotFoundException("Cannot read prism data '{0}'.  The file could not be found", prismFile); 
            }

            PrismData prismData = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(prismFile))
                {
                    prismData = PrismDataReaderV1.ReadXml(reader);
                }
            }
            catch (Exception e)
            {
                throw new InvalidContentException(String.Format("Unable to read prism data. {0}", e.Message), new ContentIdentity(fileName), e);
            }

            prismData.PrismFile = prismFile;
            prismData.PngFile = new ParsedPath(context.IntermediateDirectory, PathType.Directory).SetFileAndExtension(prismFile.File + ".png");
            prismData.PinboardFile = prismData.PinboardFile.MakeFullPath(prismFile);

            if (prismData.SvgDirectory != null)
                prismData.SvgDirectory = prismData.SvgDirectory.MakeFullPath(prismFile);

            return prismData;
        }
    }
}
