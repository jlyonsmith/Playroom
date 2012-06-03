using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Playroom
{
    public abstract class ContentTypeWriter
    {
        public Type Type { get; private set; }

        public ContentTypeWriter(Type type)
        {
            this.Type = type;
        }

        public abstract ContentTypeReaderName GetReaderName();
        public abstract void Write(ContentWriter writer, object value);
    }

    public abstract class ContentTypeWriter<T> : ContentTypeWriter
    {
        public ContentTypeWriter() : base(typeof(T))
        {
        }

        public override void Write(ContentWriter writer, object value)
        {
            this.Write(writer, (T)value);
        }

        public abstract void Write(ContentWriter writer, T value);
    }
}
