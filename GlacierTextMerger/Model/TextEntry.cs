using System.Collections.Generic;

namespace GlacierTextMerger.Model
{
    public class TextEntry
    {
        public uint Hash { get; set; }
        public string Entry { get; set; }
        public List<string> Flags { get; private set; }

        public TextEntry()
        {
            Flags = new List<string>();
        }
    }
}
