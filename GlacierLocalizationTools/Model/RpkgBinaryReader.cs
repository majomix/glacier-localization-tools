using System;
using System.IO;

namespace GlacierLocalizationTools.Model
{
    public class RpkgBinaryReader : BinaryReader
    {
        private int myVersion;

        public RpkgBinaryReader(int version, FileStream fileStream)
            : base(fileStream)
        {
            myVersion = version;
        }

        public RpkgFileStructure ReadHeader()
        {
            var rpkgFile = new RpkgFileStructure();

            rpkgFile.Signature = new string(ReadChars(4));
            if (rpkgFile.Version == RpkgVersion.Rpkg2)
            {
                rpkgFile.Version2ArchiveNumber = ReadBytes(9);
            }
            rpkgFile.NumberOfFiles = ReadUInt32();
            rpkgFile.ResourceTableOffset = ReadUInt32();
            rpkgFile.ResourceTableSize = ReadUInt32();

            if (rpkgFile.NumberOfFiles == 0)
                return rpkgFile;

            if (myVersion == 0)
            {
                UInt32 zero = ReadUInt32();
                if (zero == 0)
                {
                    rpkgFile.BaseVersionZero = true;
                }
                else
                {
                    BaseStream.Seek(-4, SeekOrigin.Current);
                }
            }
            else if (myVersion == 1)
            {
                uint numberOfLongs = ReadUInt32();
                for(int i = 0; i < numberOfLongs; i++)
                {
                    rpkgFile.PatchUnknownValues.Add(ReadUInt64());
                }
            }

            return rpkgFile;
        }

        public RpkgEntry ReadEntry()
        {
            RpkgEntry entry = new RpkgEntry();
            entry.Hash = ReadUInt64();
            entry.Offset = ReadUInt64();
            entry.CompressedSizeWithFlags = ReadUInt32();

            return entry;
        }

        public RpkgEntryInfo ReadEntryInfo()
        {
            RpkgEntryInfo entryInfo = new RpkgEntryInfo();

            entryInfo.Signature = new string(ReadChars(4));
            entryInfo.AdditionalDataSize = ReadUInt32();
            entryInfo.StateDataSize = ReadUInt32();
            entryInfo.DecompressedDataSize = ReadUInt32();
            entryInfo.SystemMemoryRequirement = ReadUInt32();
            entryInfo.VideoMemoryRequirement = ReadUInt32();
            entryInfo.AdditionalData = ReadBytes((int) entryInfo.AdditionalDataSize);
            entryInfo.StateData = ReadBytes((int)entryInfo.StateDataSize);

            return entryInfo;
        }

        public byte[] ReadCompressedBytes(int count, uint decompressedSize)
        {
            byte[] encryptionKey = new byte[] { 0xDC, 0x45, 0xA6, 0x9C, 0xD3, 0x72, 0x4C, 0xAB };

            byte[] input = ReadBytes(count);
            byte[] output = new byte[(int)decompressedSize];

            for (int i = 0; i < input.Length; i++)
            {
                input[i] ^= encryptionKey[i % 8];
            }

            int result = LZ4Handler.LZ4_decompress_safe(input, output, input.Length, (int)decompressedSize);

            if(result != decompressedSize)
            {
                throw new ArgumentException();
            }

            return output;
        }
    }
}
