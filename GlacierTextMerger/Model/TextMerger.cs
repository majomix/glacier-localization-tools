using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GlacierTextMerger.Model
{
    public class TextMerger
    {
        private Dictionary<string, List<TextSection>> languages = new Dictionary<string, List<TextSection>>();

        public void LoadTextFolders(string[] directories)
        {
            foreach (var languagePair in GetLanguageMap())
            {
                List<TextSection> currentLanguage = new List<TextSection>();

                foreach (string directory in directories)
                {
                    using (FileStream fileStream = File.Open(directory + @"\" + languagePair.Value + ".txt", FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(fileStream, Encoding.UTF8))
                        {
                            string line = null;

                            TextSection currentSection = null;

                            while ((line = reader.ReadLine()) != null)
                            {
                                // new section
                                if (line.StartsWith("[") && line.EndsWith("]"))
                                {
                                    string filename = line.Split('[', ']')[1];

                                    currentSection = currentLanguage.SingleOrDefault(_ => _.Name == filename);

                                    if(currentSection == null)
                                    {
                                        currentSection = new TextSection() { Name = filename };
                                        currentLanguage.Add(currentSection);
                                    }
                                }
                                else if (currentSection != null)
                                {
                                    string[] tokens = line.Split(new Char[] { '\t' }, 2);

                                    if (tokens.Count() == 2)
                                    {
                                        UInt32 hash = Convert.ToUInt32(tokens[0], 16);
                                        TextEntry currentEntry = currentSection.Entries.SingleOrDefault(_ => _.Hash == hash);

                                        if(currentEntry == null)
                                        {
                                            currentEntry = new TextEntry() { Hash = hash };
                                            currentSection.Entries.Add(currentEntry);
                                        }

                                        currentEntry.Entry = tokens[1];
                                        currentEntry.Flags.Add(new DirectoryInfo(directory).Name);
                                    }
                                }
                            }
                        }
                    }
                }

                languages.Add(languagePair.Value, currentLanguage);
            }
        }

        public void WriteCompactedTexts(string directory)
        {
            Directory.CreateDirectory(directory);

            foreach (var language in languages)
            {
                using (FileStream compactedFileStream = File.Open(directory + @"\" + language.Key + ".txt", FileMode.Create))
                {
                    using (StreamWriter compactedWriter = new StreamWriter(compactedFileStream, Encoding.UTF8))
                    {
                        using (FileStream mappingFileStream = File.Open(directory + @"\" + language.Key + @"_Mapping.txt", FileMode.Create))
                        {
                            using (StreamWriter mappingWriter = new StreamWriter(mappingFileStream, Encoding.UTF8))
                            {
                                foreach (var textSection in language.Value)
                                {
                                    compactedWriter.WriteLine("[{0}]", textSection.Name);
                                    mappingWriter.WriteLine("[{0}]", textSection.Name);

                                    foreach (TextEntry textEntry in textSection.Entries)
                                    {
                                        compactedWriter.WriteLine("{0:X8}\t{1}", textEntry.Hash, textEntry.Entry);
                                        mappingWriter.WriteLine("{0:X8}\t{1}", textEntry.Hash, string.Join(",", textEntry.Flags));
                                    }

                                    compactedWriter.WriteLine();
                                    mappingWriter.WriteLine();
                                }
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
