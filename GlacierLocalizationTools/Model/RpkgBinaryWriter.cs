using System;
using System.IO;
using System.Text;

namespace GlacierLocalizationTools.Model
{
    public class RpkgBinaryWriter : BinaryWriter
    {
        private int myVersion;

        public RpkgBinaryWriter(int version, Stream stream)
            : base(stream)
        {
            myVersion = version;
        }

        public override void Write(string value)
        {
            Write(Encoding.ASCII.GetBytes(value));
        }

        public void Write(RpkgFileStructure header)
        {
            Write(header.Signature);
            Write(header.NumberOfFiles);
            Write(header.ResourceTableOffset);
            Write(header.ResourceTableSize);

            if (myVersion == 0)
            {
                if (header.BaseVersionZero)
                {
                    Write((UInt32)0);
                }
            }
            else if (myVersion == 1)
            {
                Write(header.PatchUnknownValues.Count);
                foreach (ulong patchUnknownValue in header.PatchUnknownValues)
                {
                    Write(patchUnknownValue);
                }
            }
        }

        public void Write(RpkgEntry entry)
        {
            Write(entry.Hash);
            Write(entry.Offset);
            Write(entry.CompressedSizeWithFlags);
        }

        public void Write(RpkgEntryInfo entryInfo)
        {
            Write(entryInfo.Signature);
            Write(entryInfo.AdditionalDataSize);
            Write(entryInfo.StateDataSize);
            Write(entryInfo.DecompressedDataSize);
            Write(entryInfo.SystemMemoryRequirement);
            Write(entryInfo.VideoMemoryRequirement);
            Write(entryInfo.AdditionalData);
            Write(entryInfo.StateData);
        }

        public int WriteCompressedBytes(byte[] input)
        {
            byte[] encryptionKey = new byte[] { 0xDC, 0x45, 0xA6, 0x9C, 0xD3, 0x72, 0x4C, 0xAB };
            byte[] outputBuffer = new byte[input.Length * 2];

            int compressedSize = LZ4Handler.LZ4_compress(input, outputBuffer, input.Length);

            for (int i = 0; i < compressedSize; i++)
            {
                outputBuffer[i] ^= encryptionKey[i % 8];
            }

            Write(outputBuffer, 0, compressedSize);

            return compressedSize;
        }
    }
}
