using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace GlacierTextConverter.Model
{
    public class TextConverter
    {
        public List<DatTextFile> Files { get; set; }

        public void LoadDatFolder(string directory, IFileVersionSpecifications specifications)
        {
            Files = new List<DatTextFile>();

            foreach (string filePath in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                try
                {
                    Files.Add(new DatTextFile() { LanguageSections = LoadDatFile(filePath, specifications), Name = Path.GetFileName(filePath) });
                    break;
                }
                catch (Exception e) { Console.WriteLine(filePath); }
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
                            break;
                            section.Entries.Add(entry);
                        }
                    }
                    break;
                }
            }

            return languageSections;
        }

        public void LoadTextFolder(string directory)
        {
            Files = new List<DatTextFile>();

            foreach (var languagePair in GetLanguageMap())
            {
                using (FileStream fileStream = File.Open(directory + @"\" + languagePair.Value + ".txt", FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        string line = null;

                        LanguageSection currentSection = null;

                        while ((line = reader.ReadLine()) != null)
                        {
                            // new section
                            if (line.StartsWith("[") && line.EndsWith("]"))
                            {
                                currentSection = new LanguageSection();

                                string filename = line.Split('[', ']')[1];

                                DatTextFile file = Files.Where(_ => _.Name == filename).SingleOrDefault();

                                if (file == null)
                                {
                                    file = new DatTextFile() { Name = filename, LanguageSections = new List<LanguageSection>() };
                                    Files.Add(file);
                                }

                                file.LanguageSections.Add(currentSection);
                            }
                            else if (currentSection != null)
                            {
                                string[] tokens = line.Split(new Char[] { '\t' }, 2);

                                if (tokens.Count() == 2)
                                {
                                    TextEntry entry = new TextEntry()
                                    {
                                        Hash = Convert.ToUInt32(tokens[0], 16),
                                        Entry = tokens[1]
                                    };
                                    currentSection.Entries.Add(entry);
                                }
                            }
                        }
                    }
                }
            }
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

        public void WriteDatFolder(string directory, IFileVersionSpecifications specifications)
        {
            if (Files == null) return;

            Directory.CreateDirectory(directory);

            foreach(DatTextFile file in Files)
            {
                using (FileStream fileStream = File.Open(directory + @"\" + file.Name, FileMode.Create))
                {
                    using (GlacierBinaryWriter writer = new GlacierBinaryWriter(fileStream, Encoding.UTF8, specifications.CypherStrategy))
                    {
                        for (int i = 0; i < specifications.NumberOfLanguages; i++)
                        {
                            writer.Write((UInt32)0);
                        }

                        for (int i = 0; i < specifications.NumberOfLanguages; i++)
                        {
                            LanguageSection section = file.LanguageSections[i];
                            
                            if (section.Entries.Count != 0)
                            {
                                section.StartingOffset = (uint) writer.BaseStream.Position;

                                writer.Write(section.Entries.Count);

                                foreach (TextEntry entry in section.Entries)
                                {
                                    writer.Write(entry.Hash);
                                    writer.Write(entry.Entry);
                                }
                            }
                            else
                            {
                                section.StartingOffset = uint.MaxValue;
                            }
                        }

                        writer.BaseStream.Seek(0, SeekOrigin.Begin);
                        for (int i = 0; i < specifications.NumberOfLanguages; i++)
                        {
                            LanguageSection section = file.LanguageSections[i];

                            writer.Write(section.StartingOffset);
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
