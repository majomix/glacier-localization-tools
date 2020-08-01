using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GlacierTextConverter.Model
{
    public class TextConverter
    {
        private readonly HitmanVersion _version;
        public List<LocrTextFile> LocrFiles { get; }
        public List<DlgeTextFile> DlgeFiles { get; }
        public List<RtlvTextFile> RtlvFiles { get; }
        private readonly Dictionary<UInt32, HashHolder> _replacement;
        private readonly List<string> _categories;

        public TextConverter(HitmanVersion version)
        {
            _version = version;
            LocrFiles = new List<LocrTextFile>();
            DlgeFiles = new List<DlgeTextFile>();
            RtlvFiles = new List<RtlvTextFile>();
            _replacement = new Dictionary<UInt32, HashHolder>();
            _categories = new List<string>();
        }

        public void LoadLocrFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            LoadGameDataFolder(directory, groupByDirectories, categoryDirectory, (filePath, category) => LocrFiles.Add(LoadLocrFile(filePath, category)));
        }

        public void LoadDlgeFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            LoadGameDataFolder(directory, groupByDirectories, categoryDirectory, (filePath, category) => DlgeFiles.Add(LoadDlgeFile(filePath, category)));
        }

        public void LoadRtlvFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            LoadGameDataFolder(directory, groupByDirectories, categoryDirectory, (filePath, category) => RtlvFiles.Add(LoadRtlvFile(filePath, category)));
        }

        public LocrTextFile LoadLocrFile(string path, string category)
        {
            var file = new LocrTextFile();

            using (var openFile = File.Open(path, FileMode.Open))
            using (var reader = new GlacierLocrBinaryReader(openFile, _version, file))
            {
                var languageSections = new List<LanguageSection>();

                for (var i = 0; i < reader.NumberOfLanguages; i++)
                {
                    var section = new LanguageSection();
                    section.StartingOffset = reader.ReadUInt32();

                    languageSections.Add(section);
                }

                for (var i = 0; i < reader.NumberOfLanguages; i++)
                {
                    var section = languageSections[i];
                    if (section.StartingOffset != uint.MaxValue)
                    {
                        var numberOfEntries = reader.ReadInt32();

                        for (int y = 0; y < numberOfEntries; y++)
                        {
                            var entry = new TextEntry();
                            entry.Hash = reader.ReadUInt32();
                            entry.Entry = reader.ReadString();
                            section.Entries.Add(entry);
                        }
                    }
                }

                file.Name = Path.GetFileName(path);
                file.Category = category;
                file.LanguageSections = languageSections;
                file.CypherStrategy = reader.CypherStrategy;
            }

            return file;
        }

        public DlgeTextFile LoadDlgeFile(string path, string category)
        {
            var dlgeFile = new DlgeTextFile
            {
                Name = Path.GetFileName(path),
                Category = category
            };
            DlgeStructure structure;

            using (var file = File.Open(path, FileMode.Open))
            using (var reader = new GlacierDlgeBinaryReader(file, _version))
            {
                int numberOfLanguages = 12;
                ICypherStrategy cypherStrategy = new CypherStrategyTEA();

                reader.ReadHeader();
                int iteration = 0;
                var hasAnyStrings = false;
                while (reader.HasText())
                {
                    structure = reader.ReadStructure(iteration);
                    int nonEmptyStrings = 0;

                    for (int i = 0; i < numberOfLanguages; i++)
                    {
                        structure.Dialogues[i] = reader.ReadString(cypherStrategy);

                        if (!string.IsNullOrEmpty(structure.Dialogues[i]))
                        {
                            nonEmptyStrings++;
                        }

                        if (i != numberOfLanguages - 1)
                        {
                            if (reader.ReadLanguageMetadataAndDetermineIfEmpty(i, iteration, structure))
                            {
                                i++;
                            }
                        }
                    }

                    if (nonEmptyStrings != 0)
                    {
                        hasAnyStrings = true;
                    }

                    dlgeFile.Structures.Add(structure);

                    iteration++;
                }

                if (!hasAnyStrings)
                {
                    throw new InvalidDataException("No text inside dialogue file.");
                }

                reader.BaseStream.Seek(-1, SeekOrigin.Current);
                dlgeFile.Extra = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            }

            return dlgeFile;
        }

        public RtlvTextFile LoadRtlvFile(string path, string category)
        {
            RtlvTextFile file = null;

            using (var inFile = File.Open(path, FileMode.Open))
            using (var reader = new GlacierRtlvBinaryReader(inFile, _version))
            {
                file = reader.ReadFile();
                file.Name = Path.GetFileName(path);
                file.Extra = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                file.Category = category;
            }

            return file;
        }

        public void LoadTextFolder(string directory)
        {
            var slovakContent = File.ReadAllLines(directory + @"\Slovak.txt", Encoding.Unicode);
            var englishContent = File.ReadAllLines(directory + @"\English.txt", Encoding.Unicode);
            
            for (var i = 0; i < slovakContent.Length; i++)
            {
                var slovakLine = slovakContent[i];
                var englishLine = englishContent[i];

                string[] slovakTokens = slovakLine.Split(new[] { '\t' }, 2);
                string[] englishTokens = englishLine.Split(new[] { '\t' }, 2);

                if (slovakTokens.Length == 2)
                {
                    foreach(var identifier in slovakTokens[0].Split(','))
                    {
                        var key = Convert.ToUInt32(identifier, 16);
                        HashHolder holder;

                        if (!_replacement.TryGetValue(key, out holder))
                        {
                            holder = new HashHolder();
                            _replacement.Add(key, holder);
                        }

                        holder.Add(englishTokens[1], slovakTokens[1]);
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
                using (var textFile = File.Open(directory + @"\" + languagePair.Key + ".txt", FileMode.Create))
                using (var writer = new StreamWriter(textFile, Encoding.Unicode))
                {
                    foreach (var category in _categories.OrderBy(c => c.Length).ThenBy(c => c))
                    {
                        writer.WriteLine($"[{category}]");

                        foreach (var textEntry in
                            LocrFiles
                                .Where(file => file.Category == category && file.LanguageSections.Count > languagePair.Value.Item1)
                                .SelectMany(_ => _.LanguageSections[languagePair.Value.Item1].Entries
                                    .Select(text => new {Identifier = text.Hash, Entry = text.Entry}))
                                .Concat(DlgeFiles
                                    .Where(file => file.Category == category)
                                    .SelectMany(_ => _.Structures)
                                    .Select(s => new
                                        {Identifier = s.Identifier, Entry = s.Dialogues[languagePair.Value.Item2]}))
                                .Concat(RtlvFiles.Where(_ => _ != null && _.Category == category).Select(_ => new
                                {
                                    Identifier = _.Identifier,
                                    Entry = _.Sections[languagePair.Value.Item1].Lines.First()
                                }))
                                .Where(entry => entry.Entry != null)
                                .GroupBy(entry => entry.Entry))
                        {
                            writer.Write(string.Join(",",
                                textEntry.Select(entry => string.Format("{0:X8}", entry.Identifier)).Distinct()));
                            writer.Write("\t");
                            writer.WriteLine(textEntry.Key.Replace("\r\n", "\\n"));
                        }
                    }
                }
            }
        }

        public void WriteLocrFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            if (LocrFiles == null) return;

            if (groupByDirectories)
            {
                foreach (var category in _categories)
                {
                    Directory.CreateDirectory($@"{directory}\{category}\{categoryDirectory}");
                }
            }
            else
            {
                Directory.CreateDirectory($@"{directory}\{categoryDirectory}");
            }

            foreach (var file in LocrFiles)
            {
                var path = directory + (groupByDirectories ? $@"\{file.Category}\" : @"\") + categoryDirectory + @"\" + file.Name;

                using (var fileHandle = File.Open(path, FileMode.Create))
                using (var writer = new GlacierLocrBinaryWriter(fileHandle, _version, file.CypherStrategy))
                {
                    var isEmpty = file.LanguageSections.All(section => section.Entries.Count == 0);

                    writer.WriteHeader(file);

                    foreach (var section in file.LanguageSections)
                    {
                        writer.Write((UInt32)0);
                    }

                    foreach (var section in file.LanguageSections)
                    {
                        if (section.Entries.Count != 0 || isEmpty)
                        {
                            section.StartingOffset = (uint)writer.BaseStream.Position;

                            writer.Write(section.Entries.Count);

                            foreach (var entry in section.Entries)
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

                    var startPos = 0;
                    if (file.HeaderValue.HasValue)
                    {
                        startPos = 1;
                    }
                    writer.BaseStream.Seek(startPos, SeekOrigin.Begin);

                    foreach (var section in file.LanguageSections)
                    {
                        writer.Write(section.StartingOffset);
                    }
                }
            }
        }

        public void WriteDlgeFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            if (DlgeFiles == null) return;

            if (groupByDirectories)
            {
                foreach (var category in _categories)
                {
                    Directory.CreateDirectory($@"{directory}\{category}\{categoryDirectory}");
                }
            }
            else
            {
                Directory.CreateDirectory($@"{directory}\{categoryDirectory}");
            }

            foreach (var dlgeFile in DlgeFiles)
            {
                var path = directory + (groupByDirectories ? $@"\{dlgeFile.Category}\" : @"\") + categoryDirectory + @"\" + dlgeFile.Name;
                using (var file = File.Open(path, FileMode.Create))
                {
                    using (var writer = new GlacierDlgeBinaryWriter(file, _version))
                    {
                        writer.WriteHeader();

                        for (var iteration = 0; iteration < dlgeFile.Structures.Count; iteration++)
                        {
                            var structure = dlgeFile.Structures[iteration];
                            writer.Write(structure, iteration);

                            for (int i = 0; i < structure.Dialogues.Length; i++)
                            {
                                string finalString = GetReplacementString(structure.Dialogues[i], structure.Identifier,
                                    GetLanguageMap()["English"].Item2 == i);
                                writer.Write(finalString);
                                if (finalString != null && i != structure.Dialogues.Length - 1)
                                {
                                    writer.WriteTrailingBytes(i, iteration, structure);
                                }
                            }
                        }

                        writer.Write(dlgeFile.Extra);
                    }
                }
            }
        }

        public void WriteRtlvFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            if (RtlvFiles == null) return;

            if (groupByDirectories)
            {
                foreach (var category in _categories)
                {
                    Directory.CreateDirectory($@"{directory}\{category}\{categoryDirectory}");
                }
            }
            else
            {
                Directory.CreateDirectory($@"{directory}\{categoryDirectory}");
            }

            foreach (var file in RtlvFiles)
            {
                var path = directory + (groupByDirectories ? $@"\{file.Category}\" : @"\") + categoryDirectory + @"\" + file.Name;

                using (var fileHandle = File.Open(path, FileMode.Create))
                using (var writer = new GlacierRtlvBinaryWriter(fileHandle))
                {
                    writer.Write(file);

                    for (var i = 0; i < file.Sections.Count; i++)
                    {
                        var offsetAtBeginningOfSection = writer.BaseStream.Position;
                        string finalString = GetReplacementString(file.Sections[i].Lines.First(), file.Identifier, GetLanguageMap()["English"].Item1 == i);
                        writer.Write(finalString);
                        var offsetAtEndOfSection = writer.BaseStream.Position;
                        var sectionLength = offsetAtEndOfSection - offsetAtBeginningOfSection - 4;

                        if (_version == HitmanVersion.Version2 && sectionLength == 4)
                        {
                            sectionLength = 0;
                        }

                        file.Sections[i].StartingOffset = (int)offsetAtBeginningOfSection - 12;
                        file.Sections[i].SectionLength = (short)sectionLength;
                    }

                    file.FileSize = (UInt32)writer.BaseStream.Position;

                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                    writer.Write(file);

                    writer.BaseStream.Seek(0, SeekOrigin.End);
                    writer.Write(file.Extra);
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

        private void LoadGameDataFolder(string directory, bool groupByDirectories, string categoryDirectory, Action<string, string> action)
        {
            if (!groupByDirectories)
            {
                foreach (string filePath in Directory.GetFiles(directory + @"\" + categoryDirectory, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        action(filePath, null);
                    }
                    catch (Exception)
                    {
                        //File.Delete(filePath);
                        Console.WriteLine(filePath);
                    }
                }
            }
            else
            {
                foreach (var subdirectory in Directory.GetDirectories(directory))
                {
                    var category = subdirectory.Split(new [] { directory + @"\" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    if (!_categories.Contains(category))
                    {
                        _categories.Add(category);
                    }

                    foreach (var filePath in Directory.GetFiles(subdirectory + @"\" + categoryDirectory, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            action(filePath, category);
                        }
                        catch (Exception e)
                        {
                            File.Delete(filePath);
                            //var name = Path.GetFileName(filePath);
                            //File.Copy(filePath, @"D:\deleted\" + name);
                            //Console.WriteLine(filePath);
                        }
                    }
                }
            }
        }

        private string GetReplacementString(string entry, UInt32 hash, bool shouldReplace)
        {
            if (entry == null) return entry;

            string finalString = entry;

            if (shouldReplace && _replacement.ContainsKey(hash))
            {
                var holder = _replacement[hash];
                finalString = holder.GetReplacement(entry);
            }

            return finalString;
        }
    }
}
