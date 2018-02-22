using System;
using System.Collections.Generic;

namespace GlacierLocalizationTools.Model
{
    public class RpkgFileStructure
    {
        private string mySignature;

        public string Signature
        {
            get { return mySignature; }
            set
            {
                if(value != "GKPR")
                {
                    throw new NotSupportedException();
                }
                else
                {
                    mySignature = value;
                }
            }
        }
        public uint NumberOfFiles { get; set; }
        public uint ResourceTableOffset { get; set; }
        public uint ResourceTableSize { get; set; }
        public bool BaseVersionZero { get; set; }
        public List<UInt64> PatchUnknownValues { get; private set; }
        public List<RpkgEntry> Entries { get; private set; }

        public byte[] EncryptionKey { get { return new byte[] { 0xDC, 0x45, 0xA6, 0x9C, 0xD3, 0x72, 0x4C, 0xAB }; } }

        public RpkgFileStructure()
        {
            PatchUnknownValues = new List<UInt64>();
            Entries = new List<RpkgEntry>();
        }
    }
}
