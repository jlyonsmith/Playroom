using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
	public class AudioContent
	{
		public TimeSpan Duration { get; set; }
		public AudioFileType FileType { get; set; }
		public AudioFormat AudioFormat { get; set; }
		public int LoopStart { get; set; }
		public int LoopEnd { get; set; }
		public byte[] Data { get; set; }

		public static AudioContent FromFile(string file)
		{
			AudioContent ac = new AudioContent();

			using (BinaryReader br = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read)))
			{
				char[] id = br.ReadChars(4);

				if (!id.SequenceEqual(new char[] { 'R', 'I', 'F', 'F' }))
					throw new FormatException("Missing RIFF header");

				br.ReadUInt32(); // chunk size

				id = br.ReadChars(4);

				if (!id.SequenceEqual(new char[] { 'W', 'A', 'V', 'E' }))
					throw new FormatException("Only WAVE format currently supported");

				id = br.ReadChars(4);

				if (!id.SequenceEqual(new char[] { 'f', 'm', 't', ' ' }))
					throw new FormatException("Missing 'fmt ' sub-chunk");

				int size = (int)br.ReadUInt32(); // sub-chunk size

				if (size != 16)
					throw new FormatException("Unexpected format size");

				ac.FileType = AudioFileType.Wav;

				AudioFormat af = new AudioFormat();

				br.ReadUInt16();
				af.Channels = (int)br.ReadUInt16();
				af.SampleRate = (int)br.ReadUInt32();
				af.AverageBytesPerSecond = (int)br.ReadUInt32();
				af.BlockAlign = (int)br.ReadUInt16();
				af.BitsPerSample = (int)br.ReadUInt16();

				ac.AudioFormat = af;

				id = br.ReadChars(4);

				if (!id.SequenceEqual(new char[] { 'd', 'a', 't', 'a' }))
					throw new FormatException("Missing 'data' sub-chunk");

				size = (int)br.ReadUInt32(); // sub-chunk size

				ac.Data = br.ReadBytes(size);

				ac.LoopStart = 0;
				ac.LoopEnd = ac.Data.Length / af.BlockAlign;
				ac.Duration = new TimeSpan(0, 0, 0, 0, (int)(((double)ac.LoopEnd / (double)af.SampleRate + 0.0005) * 1000));
			}

			return ac;
		}
	}
}

