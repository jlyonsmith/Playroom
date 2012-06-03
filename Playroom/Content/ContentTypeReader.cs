﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Playroom
{
    // TODO: Add methods to have these return their name based on the metadata

    public abstract class ContentTypeReader
    {
        public Type Type { get; private set; }

        public ContentTypeReader(Type type)
        {
            this.Type = type;
        }

        public abstract void Read(ContentReader reader, out object value);
    }

    public abstract class ContentTypeReader<T> : ContentTypeReader
    {
        public ContentTypeReader() : base(typeof(T))
        {
        }

        public override void Read(ContentReader reader, out object value)
        {
            T obj;

            this.Read(reader, out obj);

            value = (T)obj;
        }

        public abstract void Read(ContentReader reader, out T value);
    }
}
