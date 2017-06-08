using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlacierLocalizationTools.Model
{
    public class RpkgEntryInfo
    {
        public string Signature { get; set; }
        public UInt32 AdditionalDataSize { get; set; }
        public UInt32 StateDataSize { get; set; }
        public UInt32 DecompressedDataSize { get; set; }
        public UInt32 SystemMemoryRequirement { get; set; }
        public UInt32 VideoMemoryRequirement { get; set; }
        public byte[] AdditionalData { get; set; }
        public byte[] StateData { get; set; }
    }
}
