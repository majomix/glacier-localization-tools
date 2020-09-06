using System;

namespace GlacierLocalizationTools.Model
{
    public class RpkgEntry
    {
        public UInt64 Hash { get; set; }
        public UInt64 Offset { get; set; }
        public string Name { get; set; }
        public UInt32 CompressedSizeWithFlags
        {
            get
            {
                UInt32 size = (UInt32)CompressedSize;
                if (IsEncrypted) size |= 0x80000000;
                if (IsUnknownFlag) size |= 0x40000000;

                return size;
            }
            set
            {
                IsEncrypted = (value & 0x80000000) == 0x80000000;
                IsUnknownFlag = (value & 0x40000000) == 0x40000000;
                CompressedSize = (Int32)(value & 0x3FFFFFFF);
                IsCompressed = CompressedSize != 0;
            }
        }
        public RpkgEntryInfo Info { get; set; }

        public bool IsEncrypted { get; set; }
        public bool IsUnknownFlag { get; set; }
        public bool IsCompressed { get; set; }
        public Int32 CompressedSize { get; set; }

        public bool Extract { get; set; }
        public string Import { get; set; }
        public byte[] ImportRawData { get; set; }
    }
}
