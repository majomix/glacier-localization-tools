using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
