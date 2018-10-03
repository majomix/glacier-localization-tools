using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GlacierTextConverter.Model
{
    public class TextConverter
    {
        public List<LocrTextFile> LocrFiles { get; private set; }
        public List<DlgeTextFile> DlgeFiles { get; private set; }
        public List<RtlvTextFile> RtlvFiles { get; }
        public Dictionary<UInt32, string> Replacement { get; private set; }

        public TextConverter()
        {
            LocrFiles = new List<LocrTextFile>();
            DlgeFiles = new List<DlgeTextFile>();
            RtlvFiles = new List<RtlvTextFile>();
            Replacement = new Dictionary<UInt32, string>();
        }

        public void LoadLocrFolder(string directory)
        {
            LoadGameDataFolder(directory, filePath => LocrFiles.Add(LoadLocrFile(filePath)));
        }

        public void LoadDlgeFolder(string directory)
        {
            LoadGameDataFolder(directory, filePath => DlgeFiles.Add(new DlgeTextFile() { Structure = LoadDlgeFile(filePath), Name = Path.GetFileName(filePath) }));
        }

        public void LoadRtlvFolder(string directory)
        {
            LoadGameDataFolder(directory, filePath => RtlvFiles.Add(LoadRtlvFile(filePath)));
        }

        public LocrTextFile LoadLocrFile(string path)
        {
            LocrTextFile file = null;

            using (GlacierLocrBinaryReader reader = new GlacierLocrBinaryReader(File.Open(path, FileMode.Open), Encoding.Unicode))
            {
                List<LanguageSection> languageSections = new List<LanguageSection>();

                for (int i = 0; i < reader.NumberOfLanguages; i++)
                {
                    LanguageSection section = new LanguageSection();
                    section.StartingOffset = reader.ReadUInt32();

                    languageSections.Add(section);
                }

                for (int i = 0; i < reader.NumberOfLanguages; i++)
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

                file = new LocrTextFile()
                {
                    Name = Path.GetFileName(path),
                    LanguageSections = languageSections,
                    CypherStrategy = reader.CypherStrategy
                };
            }

            return file;
        }

        public DlgeStructure LoadDlgeFile(string path)
        {
            DlgeStructure structure = null;

            using (GlacierDlgeBinaryReader reader = new GlacierDlgeBinaryReader(File.Open(path, FileMode.Open), Encoding.Unicode))
            {
                int numberOfLanguages = 12;
                ICypherStrategy cypherStrategy = new CypherStrategyTEA();

                structure = reader.ReadStructure();
                int nonEmptyStrings = 0;

                for (int i = 0; i < numberOfLanguages; i++)
                {
                    structure.Dialogues[i] = reader.ReadString(cypherStrategy);

                    if(!string.IsNullOrEmpty(structure.Dialogues[i]))
                    {
                        nonEmptyStrings++;
                    }

                    if (i != numberOfLanguages - 1)
                    {
                        if (reader.ReadLanguageMetadataAndDetermineIfEmpty(i))
                        {
                            i++;
                        }
                    }
                }

                if(nonEmptyStrings == 0)
                {
                    throw new InvalidDataException("No text inside dialogue file.");
                }

                structure.Extra = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            }

            return structure;
        }

        public RtlvTextFile LoadRtlvFile(string path)
        {
            RtlvTextFile file = null;

            var extension = Path.GetExtension(path);

            if (extension != null && extension.Contains("patch"))
            {
                using (var reader = new GlacierRtlvBinaryReader(File.Open(path, FileMode.Open), Encoding.Unicode))
                {
                    file = reader.ReadFile();
                    file.Name = Path.GetFileName(path);
                }
            }

            return file;
        }

        public void LoadTextFolder(string directory)
        {
            using (StreamReader reader = new StreamReader(File.Open(directory + @"\Slovak.txt", FileMode.Open), Encoding.UTF8))
            {
                string line = null;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] tokens = line.Split(new Char[] { '\t' }, 2);

                    if (tokens.Count() == 2)
                    {
                        foreach(var identifier in tokens[0].Split(new Char[] { ',' }))
                        {
                            Replacement[Convert.ToUInt32(identifier, 16)] = tokens[1];
                        }
                    }
                }
            }
        }

        public void WriteCombinedTextFile(string directory)
        {
            if(LocrFiles == null || DlgeFiles == null) return;

            Directory.CreateDirectory(directory);

            foreach (var languagePair in GetLanguageMap())
            {
                using (StreamWriter writer = new StreamWriter(File.Open(directory + @"\" + languagePair.Key + ".txt", FileMode.Create), Encoding.UTF8))
                {
                    foreach (var textEntry in 
                        LocrFiles
                            .Where(file => file.LanguageSections.Count > languagePair.Value.Item1)
                            .SelectMany(_ => _.LanguageSections[languagePair.Value.Item1].Entries
                                .Select(text => new { Identifier = text.Hash, Entry = text.Entry }))
                        .Concat(DlgeFiles
                            .Select(_ => new { Identifier = _.Structure.Identifier, Entry = _.Structure.Dialogues[languagePair.Value.Item2] }))
                        .Concat(RtlvFiles.Where(_ => _ != null).Select(_ => new { Identifier = _.Identifier, Entry = _.Sections[languagePair.Value.Item1].Lines.First() }))
                        .Where(entry => entry.Entry != null)
                        .GroupBy(entry => entry.Entry))
                    {
                        writer.Write(string.Join(",", textEntry.Select(entry => string.Format("{0:X8}", entry.Identifier)).Distinct()));
                        writer.Write("\t");
                        writer.WriteLine(textEntry.Key.Replace("\r\n", "\\n"));
                    }
                }
            }
        }

        public void WriteLocrFolder(string directory)
        {
            if (LocrFiles == null) return;

            Directory.CreateDirectory(directory);

            foreach(LocrTextFile file in LocrFiles)
            {
                using (GlacierLocrBinaryWriter writer = new GlacierLocrBinaryWriter(File.Open(directory + @"\" + file.Name, FileMode.Create), Encoding.UTF8, file.CypherStrategy))
                {
                    bool isEmpty = file.LanguageSections.All(section => section.Entries.Count == 0);

                    foreach (var section in file.LanguageSections)
                    {
                        writer.Write((UInt32)0);
                    }

                    foreach (LanguageSection section in file.LanguageSections)
                    {
                        if (section.Entries.Count != 0 || isEmpty)
                        {
                            section.StartingOffset = (uint)writer.BaseStream.Position;

                            writer.Write(section.Entries.Count);

                            foreach (TextEntry entry in section.Entries)
                            {
                                writer.Write(entry.Hash);
                                writer.Write(GetReplacementString(entry.Entry, entry.Hash, file.LanguageSections[GetLanguageMap()["English"].Item1] == section));
                            }
                        }
                        else
                        {
                            section.StartingOffset = uint.MaxValue;
                        }
                    }

                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var section in file.LanguageSections)
                    {
                        writer.Write(section.StartingOffset);
                    }
                }
            }
        }

        public void WriteDlgeFolder(string directory)
        {
            if (DlgeFiles == null) return;

            Directory.CreateDirectory(directory);

            foreach (DlgeTextFile file in DlgeFiles)
            {
                using (GlacierDlgeBinaryWriter writer = new GlacierDlgeBinaryWriter(File.Open(directory + @"\" + file.Name, FileMode.Create), Encoding.UTF8))
                {
                    writer.Write(file.Structure);

                    for (int i = 0; i < file.Structure.Dialogues.Length; i++)
                    {
                        string finalString = GetReplacementString(file.Structure.Dialogues[i], file.Structure.Identifier, GetLanguageMap()["English"].Item2 == i);
                        writer.Write(finalString);
                        if (finalString != null && i != file.Structure.Dialogues.Length - 1)
                        {
                            writer.WriteTrailingBytes(i);
                        }
                    }

                    writer.Write(file.Structure.Extra);
                }
            }
        }

        public void WriteRtlvFolder(string directory)
        {
            if (RtlvFiles == null) return;

            Directory.CreateDirectory(directory);

            foreach (var file in RtlvFiles)
            {
                using (var writer = new GlacierRtlvBinaryWriter(File.Open(directory + @"\" + file.Name, FileMode.Create), Encoding.UTF8))
                {
                    writer.Write(file);

                    for (var i = 0; i < file.Sections.Count; i++)
                    {
                        var offsetAtBeginningOfSection = writer.BaseStream.Position;
                        string finalString = GetReplacementString(file.Sections[i].Lines.First(), file.Identifier, GetLanguageMap()["English"].Item1 == i);
                        writer.Write(finalString);
                        var offsetAtEndOfSection = writer.BaseStream.Position;
                        var sectionLength = offsetAtEndOfSection - offsetAtEndOfSection;
                    }

                    //for (int i = 0; i < file.Structure.Dialogues.Length; i++)
                    //{
                    //    string finalString = GetReplacementString(file.Structure.Dialogues[i], file.Structure.Identifier, GetLanguageMap()["English"].Item2 == i);
                    //    writer.Write(finalString);
                    //    if (finalString != null && i != file.Structure.Dialogues.Length - 1)
                    //    {
                    //        writer.WriteTrailingBytes(i);
                    //    }
                    //}

                    //writer.Write(file.Structure.Extra);
                }
            }
        }

        public Dictionary<string, Tuple<int, int>> GetLanguageMap()
        {
            return new Dictionary<string, Tuple<int, int>>()
            {
                { "Empty", new Tuple<int, int>(0, 10) },
                { "English", new Tuple<int, int>(1, 0) },
                { "French", new Tuple<int, int>(2, 1 )},
                { "Italian", new Tuple<int, int>(3, 2) },
                { "German", new Tuple<int, int>(4, 3) },
                { "Spanish", new Tuple<int, int>(5, 4) },
                { "Russian", new Tuple<int, int>(6, 5) },
                { "Mexican", new Tuple<int, int>(7, 6) },
                { "Portugese", new Tuple<int, int>(8, 7) },
                { "Polish", new Tuple<int, int>(9, 8) },
                { "Japanese", new Tuple<int, int>(10, 9) },
                { "Chinese", new Tuple<int, int>(0, 11) },
            };
        }

        private void LoadGameDataFolder(string directory, Action<string> action)
        {
            foreach (string filePath in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                try
                {
                    action(filePath);
                }
                catch (Exception e)
                {
                    File.Delete(filePath);
                    Console.WriteLine(filePath);
                }
            }
        }

        private string GetReplacementString(string entry, UInt32 hash, bool extraCondition)
        {
            if (entry == null) return entry;

            string finalString = entry;

            if (extraCondition && Replacement.ContainsKey(hash))
            {
                finalString = Replacement[hash];
            }

            return finalString.Replace("\\n", "\r\n");
        }
    }
}
