﻿using System;
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
        private Dictionary<Type, int> typeTable;
        private List<ContentTypeWriter> usedTypeWriters;
        private List<object> sharedResources;

        // TODO: This list should come from reflection on the assembly
        private readonly ContentTypeWriter[] availableTypeWriters = new ContentTypeWriter[]
        {
            new Int32Writer(),
            new StringWriter(),
            new RectangleWriter(),
            new ArrayWriter<Microsoft.Xna.Framework.Rectangle>()
        };
        
        private XnbFileWriterV5(FileStream fileStream)
        {
            this.fileStream = fileStream;
            this.usedTypeWriters = new List<ContentTypeWriter>();
            this.typeTable = new Dictionary<Type, int>();
            this.sharedResources = new List<object>();
        }

        public static void WriteFile(object rootObject, ParsedPath xnbFile)
        {
            using (FileStream fileStream = new FileStream(xnbFile, FileMode.Create))
            {
                new XnbFileWriterV5(fileStream).Write(rootObject);
            }
        }

        private byte[] WriteHeaderData(int compressedSize)
        {
            byte[] data = null;

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)'X');
                    writer.Write((byte)'N');
                    writer.Write((byte)'B');
                    writer.Write((byte)'m'); // TODO: Support other platforms
                    writer.Write((byte)5);
                    writer.Write((byte)0);  // TODO: Support XNB flags
                    writer.Write(compressedSize);
                    // TODO: Support compressed files
                    // writer.Write(decompressedSize);

                    data = stream.GetBuffer().Take((int)stream.Length).ToArray();
                }
            }

            return data;
        }

        private byte[] WriteTypeReadersData(IList<ContentTypeWriter> usedTypeWriters)
        {
            byte[] data = null;
            List<string> typeReaderNames = usedTypeWriters.Select(w => w.GetReaderTypeName()).ToList();

            using (MemoryStream stream = new MemoryStream())
            {
                using (ContentWriter writer = new ContentWriter(stream, this))
                {
                    writer.WriteEncodedInt32(typeReaderNames.Count);

                    foreach (var typeReader in typeReaderNames)
                    {
                        writer.Write(typeReader.ToString());
                        writer.Write(0);
                    }

                    data = stream.GetBuffer().Take((int)stream.Length).ToArray();
                }
            }

            return data;
        }

        private byte[] WriteRootObjectData(object rootObject)
        {
            byte[] rootObjectData = null;

            using (MemoryStream stream = new MemoryStream())
            {
                using (ContentWriter writer = new ContentWriter(stream, this))
                {
                    writer.WriteObject<object>(rootObject);
                    writer.Flush();

                    stream.Flush();
                    rootObjectData = stream.GetBuffer().Take((int)stream.Length).ToArray();
                }
            }
            
            return rootObjectData;
        }

        private List<byte[]> WriteSharedResourcesData(out byte[] sharedResourcesCountData)
        {
            List<byte[]> shareResourcesData = new List<byte[]>();

            using (MemoryStream stream = new MemoryStream())
            {
                using (ContentWriter writer = new ContentWriter(stream, this))
                {
                    writer.WriteEncodedInt32(sharedResources.Count);
                    writer.Flush();

                    stream.Flush();
                    sharedResourcesCountData = stream.GetBuffer().Take((int)stream.Length).ToArray();
                }
            }

            // TODO: Support shared resources
            return shareResourcesData;
        }

        private void Write(object rootObject)
        {
            // Collect all the bytes for the various parts of the file
            byte[] rootObjectData = WriteRootObjectData(rootObject);
            byte[] sharedResourcesCountData;
            List<byte[]> sharedResourcesData = WriteSharedResourcesData(out sharedResourcesCountData);
            byte[] typeReaderData = WriteTypeReadersData(usedTypeWriters);
            int compressedSize = 10 + typeReaderData.Length + sharedResourcesCountData.Length + rootObjectData.Length + sharedResourcesData.Sum(a => a.Length);
            byte[] headerData = WriteHeaderData(compressedSize);

            // Finally, write out all the bytes in order
            using (BinaryWriter writer = new BinaryWriter(this.fileStream))
            {
                writer.Write(headerData);
                writer.Write(typeReaderData);
                writer.Write(sharedResourcesCountData);
                writer.Write(rootObjectData);

                foreach (var sharedResourceData in sharedResourcesData)
                    writer.Write(sharedResourceData); 
            }
        }

        internal ContentTypeWriter GetTypeWriter(Type type)
        {
            int typeIndex;

            return GetTypeWriter(type, out typeIndex);
        }

        internal ContentTypeWriter GetTypeWriter(Type type, out int typeIndex)
        {
            // Is it a type writer we have already used?
            if (this.typeTable.TryGetValue(type, out typeIndex))
            {
                return usedTypeWriters[typeIndex];
            }

            ContentTypeWriter typeWriter = null;

            try
            {
                typeWriter = availableTypeWriters.First(t => t.TargetType == type);
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException)
                    throw new InvalidOperationException(String.Format("No type writer for type '{0}'", type.Name), e);
                else
                    throw;
            }

            // Add it to the list of used type writers
            typeIndex = usedTypeWriters.Count;
            usedTypeWriters.Add(typeWriter);
            typeTable.Add(type, typeIndex);

            typeWriter.Initialize(this);

            return typeWriter;
        }
    }
}