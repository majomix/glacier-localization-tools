using System;
using System.Collections.Generic;

namespace GlacierTextConverter.Model
{
    public class RtlvTextFile
    {
        public byte[] Header;
        public UInt32 FileSize;
        public byte[] StaticContext;
        public UInt32 Identifier;
        public byte[] StaticContext2;
        public List<RtvlLanguageSection> Sections;
        public string Name;
        public byte[] Extra;
        public string Category;

        public RtlvTextFile()
        {
            Sections = new List<RtvlLanguageSection>();
        }
    }

    public class RtvlLanguageSection
    {
        public Int16 SectionLength;
        public Int16 Unknown;
        public Int32 Zeros;
        public Int32 StartingOffset; // -12
        public Int32 Zeros2;
        public List<string> Lines;

        public RtvlLanguageSection()
        {
            Lines = new List<string>();
        }
    }
}
