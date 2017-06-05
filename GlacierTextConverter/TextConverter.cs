using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierTextConverter
{
    public class TextConverter
    {
        public List<LanguageSection> LanguageSections { get; private set; }

        public void LoadTextFile(string path, IFileVersionSpecifications specifications)
        {
            LanguageSections = new List<LanguageSection>();

            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {
                GlacierBinaryReader reader = new GlacierBinaryReader(fileStream, Encoding.Unicode, specifications.CypherStrategy);

                for (int i = 0; i < specifications.NumberOfLanguages; i++)
                {
                    LanguageSection section = new LanguageSection();
                    section.StartingOffset = reader.ReadUInt32();

                    LanguageSections.Add(section);
                }

                for (int i = 0; i < specifications.NumberOfLanguages; i++)
                {
                    LanguageSection section = LanguageSections[i];
                    int numberOfEntries = reader.ReadInt32();

                    for(int y = 0; y < numberOfEntries; y++)
                    {
                        TextEntry entry = new TextEntry();
                        entry.Hash = reader.ReadUInt32();
                        entry.Entry = reader.ReadString();

                        section.Entries.Add(entry);
                    }
                }
            }
        }
    }
}
