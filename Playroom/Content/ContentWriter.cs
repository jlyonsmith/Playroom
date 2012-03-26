using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Playroom
{
    public sealed class ContentWriter : BinaryWriter
    {
        private const ushort XnbVersion = 0x05;

        private Dictionary<object, bool> recursionDector;
        private Dictionary<Type, int> typeTable;
        private List<ContentTypeWriter> typeWriters;

        public ContentWriter()
        {
        }

        private ContentTypeWriter CreateTypeWriter(Type type)
        {
            /*
            if (type.IsArray)
            {
                return CreateArrayTypeWriter(type);
            }
            
            if (type.IsEnum)
            {
                return CreateGenericTypeWriter(typeof(EnumWriter<>), type);
            }
            */

            return CreateTypeWriter(type);
        }

        private ContentTypeWriter GetTypeWriter(Type type, out int typeIndex)
        {
            if (this.typeTable.TryGetValue(type, out typeIndex))
            {
                return this.typeWriters[typeIndex];
            }

            ContentTypeWriter typeWriter = CreateTypeWriter(type);

            this.typeWriters.Add(typeWriter);
            this.typeTable.Add(type, typeIndex);
            
            return typeWriter;
        }

        public void WriteObject<T>(T value)
        {
            if (value == null)
            {
                base.Write7BitEncodedInt(0);
            }
            else
            {
                int num;
                ContentTypeWriter typeWriter = this.GetTypeWriter(value.GetType(), out num);

                base.Write7BitEncodedInt(num + 1);

                if (this.recursionDector.ContainsKey(value))
                {
                    throw new InvalidOperationException();
                }
                
                this.recursionDector.Add(value, true);
                this.InvokeWriter<T>(value, typeWriter);
                this.recursionDector.Remove(value);
            }
        }
    }
}
