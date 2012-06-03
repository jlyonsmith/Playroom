using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;

namespace Playroom
{
    public class ContentWriter : BinaryWriter
    {
        private Dictionary<object, bool> activeObjects;
        private Dictionary<Type, int> typeTable;
        private IList<ContentTypeWriter> availableTypeWriters;
        private List<ContentTypeWriter> usedTypeWriters;
        private List<object> sharedResources;

        public ContentWriter(Stream stream, IList<ContentTypeWriter> availableTypeWriters)
            : base(stream)
        {
            this.availableTypeWriters = availableTypeWriters;
            this.activeObjects = new Dictionary<object, bool>();
            this.typeTable = new Dictionary<Type, int>();
            this.usedTypeWriters = new List<ContentTypeWriter>();
            this.sharedResources = new List<object>();
        }

        public ReadOnlyCollection<ContentTypeReaderName> GetTypeReaderNames()
        {
            return usedTypeWriters.Select<ContentTypeWriter, ContentTypeReaderName>(c => c.GetReaderName()).ToList().AsReadOnly();
        }

        public ReadOnlyCollection<object> GetSharedResources()
        {
            return sharedResources.AsReadOnly();
        }

        public int AddSharedResource(object obj)
        {
            sharedResources.Add(obj);
            
            return sharedResources.Count - 1;
        }

        public void WriteObject<T>(T value)
        {
            if (value == null)
            {
                Write7BitEncodedInt(0);
            }
            else
            {
                int typeIndex;
                ContentTypeWriter typeWriter = GetTypeWriter(value.GetType(), out typeIndex);

                Write7BitEncodedInt(typeIndex);

                if (activeObjects.ContainsKey(value))
                    throw new InvalidOperationException("Recursive object graph detected");

                activeObjects.Add(value, true);
                InvokeWriter<T>(value, typeWriter);
                activeObjects.Remove(value);
            }
        }

        private ContentTypeWriter GetTypeWriter(Type type, out int typeIndex)
        {
            // Is it a type writer we have already used?
            if (this.typeTable.TryGetValue(type, out typeIndex))
            {
                return usedTypeWriters[typeIndex];
            }

            ContentTypeWriter typeWriter = availableTypeWriters.First(t => t.Type == type);

            // Add it to the list of used type writers
            typeIndex = usedTypeWriters.Count;
            usedTypeWriters.Add(typeWriter);
            typeTable.Add(type, typeIndex);
            
            return typeWriter;
        }

        private void InvokeWriter<T>(T value, ContentTypeWriter writer)
        {
            ContentTypeWriter<T> genericWriter = writer as ContentTypeWriter<T>;

            if (genericWriter != null)
            {
                genericWriter.Write(this, value);
            }
            else
            {
                writer.Write(this, value);
            }
        }
    }
}
