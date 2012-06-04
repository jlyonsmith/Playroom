using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Playroom
{
    public abstract class XnaBuiltinTypeWriter<T> : ContentTypeWriter<T>
    {
        public override string GetReaderTypeName()
        {
            // It looks like any type that is in the MS.Xna.Framework assembly doesn't need to be fully qualified with an assembly
            string name = GetShortTypeName(this.GetType()).Replace("Writer", "Reader") + GetGenericArgumentRuntimeTypes();

            return String.Format("Microsoft.Xna.Framework.Content.{0}", name); // ", Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553"
        }
    }

    public class Int32Writer : XnaBuiltinTypeWriter<Int32>
    {
        public override void Write(ContentWriter writer, Int32 value)
        {
            writer.Write(value);
        }
    }

    public class StringWriter : XnaBuiltinTypeWriter<String>
    {
        public override void Write(ContentWriter writer, string value)
        {
            writer.Write(value);
        }
    }

    public class RectangleWriter : XnaBuiltinTypeWriter<Rectangle>
    {
        public override void Write(ContentWriter writer, Rectangle value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Width);
            writer.Write(value.Height);
        }
    }

    public class ArrayWriter<T> : XnaBuiltinTypeWriter<T[]>
    {
        private ContentTypeWriter elementTypeWriter;

        public override void Initialize(XnbFileWriterV5 xnbWriter)
        {
            if (elementTypeWriter == null)
                elementTypeWriter = xnbWriter.GetTypeWriter(typeof(T));

            base.Initialize(xnbWriter);
        }

        public override void Write(ContentWriter writer, T[] value)
        {
            writer.Write(value.Length);

            foreach (T local in value)
            {
                writer.WriteObject<T>(local, elementTypeWriter);
            }
        }
    }
}
