using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StutterMosher
{
    public class Frame
    {
        // Byte signature for i-frames (full image frames)
        static readonly List<byte> IFrameSig = new List<byte> { 0x00, 0x01, 0xB0 };
        // Byte signature for p-frames (delta frames)
        static readonly List<byte> PFrameSig = new List<byte> { 0x00, 0x01, 0xB6 };
        // Byte signature that signals the end of frame data
        static readonly List<byte> EndOfFrame = new List<byte> { 0x30, 0x30, 0x64, 0x63 };

        public List<byte> Data { get; }

        public List<byte> Signature
        {
            get
            {
                try
                {
                    return Data.GetRange(5, 3);
                }
                catch (ArgumentException)
                {
                    return new List<byte>();
                }
            }
        }

        public bool IsIFrame => Signature.SequenceEqual(IFrameSig);
        public bool IsPFrame => Signature.SequenceEqual(PFrameSig);

        public Frame(List<byte> data)
        {
            Data = data;
        }

        public void WriteToStream(Stream stream)
        {
            byte[] data = Data.Concat(EndOfFrame).ToArray();
            stream.Write(data, 0, data.Length);
        }

        public static Frame ReadFromStream(Stream stream)
        {
            byte[] buffer = new byte[4096];
            List<byte> data = new List<byte>();

            long streamPos = stream.Position;

            // Read data until end-of-frame is found
            while (data.IndexOf(EndOfFrame) == -1)
            {
                if (stream.Read(buffer, 0, buffer.Length) > 0)
                    data.AddRange(buffer);
                else return null;
            }

            // Trim extra data
            int index = data.IndexOf(EndOfFrame);
            data.RemoveRange(index, data.Count - index);
            stream.Seek(streamPos + data.Count + 4, SeekOrigin.Begin);

            return new Frame(data);
        }
    }
}
