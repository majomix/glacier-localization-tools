using System.Collections.Generic;

namespace GlacierTextConverter.Model
{
    public class DatTextFile
    {
        public string Name { get; set; }
        public List<LanguageSection> LanguageSections { get; set; }
    }
}
