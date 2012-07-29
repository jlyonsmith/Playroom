using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Playroom;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    public abstract class XnaTypeWriter<T> : ContentTypeWriter<T>
    {
        public override string GetReaderTypeName()
        {
            string name = GetShortTypeName(this.GetType()).Replace("Writer", "Reader") + GetGenericArgumentRuntimeTypes();

            // It looks like any type that is in the Microsoft.Xna.Framework assembly doesn't need to be fully qualified with an assembly
            return String.Format("Microsoft.Xna.Framework.Content.{0}", name); // ", Microsoft.Xna.Framework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553"
        }
    }

    public class Int32Writer : XnaTypeWriter<Int32>
    {
        public override void Write(ContentWriter writer, Int32 value)
        {
            writer.Write(value);
        }
    }

    public class StringWriter : XnaTypeWriter<String>
    {
        public override void Write(ContentWriter writer, string value)
        {
            writer.Write(value);
        }
    }

    public class RectangleWriter : XnaTypeWriter<Rectangle>
    {
        public override void Write(ContentWriter writer, Rectangle value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Width);
            writer.Write(value.Height);
        }
    }

    public class ArrayWriter<T> : XnaTypeWriter<T[]>
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

    public abstract class XnaGraphicsTypeWriter<T> : ContentTypeWriter<T>
    {
        public override string GetReaderTypeName()
        {
            string name = GetShortTypeName(this.GetType()).Replace("Writer", "Reader") + GetGenericArgumentRuntimeTypes();

            return String.Format("Microsoft.Xna.Framework.Content.{0}, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553", name);
        }
    }

    public abstract class XnaAudioTypeWriter<T> : ContentTypeWriter<T>
    {
        public override string GetReaderTypeName()
        {
            string name = GetShortTypeName(this.GetType()).Replace("Writer", "Reader") + GetGenericArgumentRuntimeTypes();

            return String.Format("Microsoft.Xna.Framework.Content.{0}, Microsoft.Xna.Framework.Audio, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553", name);
        }
    }

    public abstract class BaseTextureWriter<T> : XnaGraphicsTypeWriter<T> where T : TextureContent
    {
        public BaseTextureWriter()
        {
        }

        public override void Write(ContentWriter output, T value)
        {
            BitmapContent content = value.Faces[0][0];

            this.WriteTextureHeader(output, content.Format, content.Width, content.Height, value.Faces.Count, value.Faces[0].Count);
            this.WriteTextureData(output, value);
        }

        protected virtual void WriteTextureData(ContentWriter output, T texture)
        {
            foreach (MipmapChain chain in texture.Faces)
            {
                foreach (BitmapContent content in chain)
                {
					byte[] pixelData = content.Data;

                    output.Write(pixelData.Length);
                    output.Write(pixelData);
                }
            }
        }

        protected abstract void WriteTextureHeader(ContentWriter output, SurfaceFormat format, int width, int height, int depth, int mipLevels);
    }

    public class Texture2DWriter : BaseTextureWriter<Texture2DContent>
    {
        protected override void WriteTextureHeader(ContentWriter output, SurfaceFormat format, int width, int height, int depth, int mipLevels)
        {
            output.Write((int)format);
            output.Write(width);
            output.Write(height);
            output.Write(mipLevels);
        }
    }

	public class SoundEffectWriter : XnaAudioTypeWriter<AudioContent>
	{
        public override void Write(ContentWriter output, AudioContent value)
        {
			ushort pcmFormat;

			if (value.FileType != AudioFileType.Wav)
				throw new NotImplementedException("Only WAV file support currently implemented");
			else
				pcmFormat = 1;

			output.Write((uint)18); // total size of the following WAVEFORMATEX structure
			output.Write((ushort)pcmFormat);
			output.Write((ushort)value.AudioFormat.Channels);
			output.Write((uint)value.AudioFormat.SampleRate);
			output.Write((uint)value.AudioFormat.AverageBytesPerSecond);
			output.Write((ushort)value.AudioFormat.BlockAlign);
			output.Write((ushort)value.AudioFormat.BitsPerSample);
			output.Write((ushort)0); // No extra bytes

			byte[] data = value.Data;

			output.Write(data.Length);
			output.Write(data);

			output.Write(value.LoopStart);
			output.Write(value.LoopEnd);
			output.Write(value.Duration.Milliseconds);
        }
	}
}
