using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ToolBelt;
using System.Collections.ObjectModel;

namespace Playroom
{
    public class XnbFileWriterV5
    {
        private FileStream fileStream;
        private IList<ContentTypeWriter> typeWriters;

        private XnbFileWriterV5(FileStream fileStream, IList<ContentTypeWriter> typeWriters)
        {
            this.fileStream = fileStream;
            this.typeWriters = typeWriters;
        }

        public static void WriteFile(object rootObject, IList<ContentTypeWriter> typeWriters, ParsedPath xnbFile)
        {
            using (FileStream fileStream = new FileStream(xnbFile, FileMode.Create))
            {
                new XnbFileWriterV5(fileStream, typeWriters).Write(rootObject);
            }
        }

        private void WriteHeader(int flags, int compressedSize, int decompressedSize)
        {
        }

        private void WriteTypeReaders(IList<ContentTypeReaderName> typeReaderNames)
        {
        }

        private void WriteRootObject(byte[] rootObject)
        {
        }

        private void WriteSharedResources(IList<object> shareResources)
        {
        }

        private void Write(object rootObject)
        {
            byte[] rootObjectData = null;
            ReadOnlyCollection<ContentTypeReaderName> typeReaderNames;
            ReadOnlyCollection<object> sharedResources;

            using (MemoryStream stream = new MemoryStream())
            {
                using (ContentWriter writer = new ContentWriter(stream, typeWriters))
                {
                    writer.WriteObject<object>(rootObject);
                    writer.Flush();

                    typeReaderNames = writer.GetTypeReaderNames();
                    sharedResources = writer.GetSharedResources();
                }

                stream.Flush();
                rootObjectData = stream.GetBuffer();
            }

            int flags = 0;
            int compressedSize = 0;
            int decompressedSize = 0;

            WriteHeader(flags, compressedSize, decompressedSize);
            WriteTypeReaders(typeReaderNames);
            WriteRootObject(rootObjectData);
            WriteSharedResources(sharedResources);
        }
    }
}
