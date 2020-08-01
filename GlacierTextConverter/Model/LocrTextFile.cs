using System.Collections.Generic;

namespace GlacierTextConverter.Model
{
    public class LocrTextFile
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public List<LanguageSection> LanguageSections { get; set; }
        public ICypherStrategy CypherStrategy { get; set; }
        public byte? HeaderValue { get; set; }
    }
}
