using System.Collections.Generic;

namespace GlacierTextMerger.Model
{
    public class TextSection
    {
        public List<TextEntry> Entries { get; private set; }
        public string Name { get; set; }

        public TextSection()
        {
            Entries = new List<TextEntry>();
        }
    }
}
