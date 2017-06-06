using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GlacierTextConverter
{
    public class TextConverter
    {
        public List<DatTextFile> Files { get; set; }

        public void LoadDatFolder(string directory, IFileVersionSpecifications specifications)
        {
            Files = new List<DatTextFile>();

            foreach (string filePath in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                Files.Add(new DatTextFile() { LanguageSections = LoadDatFile(filePath, specifications), Name = Path.GetFileName(filePath) });
            }
        }

        public List<LanguageSection> LoadDatFile(string path, IFileVersionSpecifications specifications)
        {
            List<LanguageSection> languageSections = new List<LanguageSection>();

            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {
                GlacierBinaryReader reader = new GlacierBinaryReader(fileStream, Encoding.Unicode, specifications.CypherStrategy);

                for (int i = 0; i < specifications.NumberOfLanguages; i++)
                {
                    LanguageSection section = new LanguageSection();
                    section.StartingOffset = reader.ReadUInt32();

                    languageSections.Add(section);
                }

                for (int i = 0; i < specifications.NumberOfLanguages; i++)
                {
                    LanguageSection section = languageSections[i];
                    if(section.StartingOffset != uint.MaxValue)
                    {
                        int numberOfEntries = reader.ReadInt32();

                        for (int y = 0; y < numberOfEntries; y++)
                        {
                            TextEntry entry = new TextEntry();
                            entry.Hash = reader.ReadUInt32();
                            entry.Entry = reader.ReadString();

                            section.Entries.Add(entry);
                        }
                    }
                }
            }

            return languageSections;
        }

        public void WriteTextFile(string directory)
        {
            if(Files == null) return;

            Directory.CreateDirectory(directory);

            foreach(var languagePair in GetLanguageMap())
            {
                using (FileStream fileStream = File.Open(directory + @"\" + languagePair.Value + ".txt", FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        foreach (DatTextFile datFile in Files)
                        {
                            if (datFile.LanguageSections.Count > languagePair.Key)
                            {
                                LanguageSection languageSection = datFile.LanguageSections[languagePair.Key];

                                writer.WriteLine("[{0}]", datFile.Name);

                                foreach (TextEntry textEntry in languageSection.Entries)
                                {
                                    writer.WriteLine("{0:X8}\t{1}", textEntry.Hash, textEntry.Entry.Replace("\r\n", "\\n"));
                                }

                                writer.WriteLine();
                            }
                        }
                    }
                }
            }
        }

        public Dictionary<int, string> GetLanguageMap()
        {
            return new Dictionary<int, string>()
            {
                { 0, "Reference" },
                { 1, "English" },
                { 2, "French" },
                { 3, "Italian" },
                { 4, "German" },
                { 5, "Spanish" },
                { 6, "Russian" },
                { 7, "Mexican" },
                { 8, "Portugese" },
                { 9, "Polish" },
                { 10, "Japanese" },
                { 11, "Empty" }
            };
        }
    }
}
