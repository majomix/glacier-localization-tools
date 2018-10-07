using System;
using System.IO;

namespace GlacierTextConverter.Model
{
    public static class Extensions
    {
        public static byte[] Reverse(this byte[] b)
        {
            Array.Reverse(b);
            return b;
        }

        public static UInt32 ReadUInt32BE(this BinaryReader binaryReader)
        {
            return BitConverter.ToUInt32(binaryReader.ReadBytesRequired(sizeof(UInt32)).Reverse(), 0);
        }

        public static void WriteUInt32BE(this BinaryWriter binaryWriter, UInt32 value)
        {
            binaryWriter.Write(BitConverter.GetBytes(value).Reverse());
        }

        public static byte[] ReadBytesRequired(this BinaryReader binaryReader, int byteCount)
        {
            var result = binaryReader.ReadBytes(byteCount);

            if (result.Length != byteCount)
                throw new EndOfStreamException($"{byteCount} bytes required from stream, but only {result.Length} returned.");

            return result;
        }
    }
}

