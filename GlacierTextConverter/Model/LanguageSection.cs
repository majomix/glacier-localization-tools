using System.Collections.Generic;

namespace GlacierTextConverter.Model
{
    public class LanguageSection
    {
        public uint StartingOffset { get; set; }
        public List<TextEntry> Entries { get; private set; }

        public LanguageSection()
        {
            Entries = new List<TextEntry>();
        }
    }
}
