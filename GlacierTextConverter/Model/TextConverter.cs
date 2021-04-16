using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        public readonly List<string> Categories;
        private readonly Dictionary<UInt32, HashHolder> _replacement;

        public TextConverter(HitmanVersion version)
        {
            _version = version;
            LocrFiles = new List<LocrTextFile>();
            DlgeFiles = new List<DlgeTextFile>();
            RtlvFiles = new List<RtlvTextFile>();
            _replacement = new Dictionary<UInt32, HashHolder>();
            Categories = new List<string>();
        }

        public void LoadLocrFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            LoadGameDataFolder(directory, groupByDirectories, categoryDirectory, (filePath, category) =>
            {
                var file = new LocrTextFile();

                using (var fileStream = File.Open(filePath, FileMode.Open))
                {
                    var fileNameToSave = Path.GetFileName(filePath);

                    LocrFiles.Add(LoadLocrFile(fileStream, file, fileNameToSave, category));
                }
            });
        }

        public void LoadLocrFromZip(ZipArchive zipArchive, bool groupByDirectories, string categoryDirectory)
        {
            if (!groupByDirectories)
                throw new NotSupportedException();

            var entries = zipArchive.Entries.Where(e => e.FullName.Contains($"/{categoryDirectory}/") && !e.FullName.EndsWith($"/"));
            foreach (var entry in entries)
            {
                var file = new LocrTextFile();

                using (var stream = entry.Open())
                {
                    var buffer = new byte[entry.Length];
                    stream.Read(buffer, 0, (int)entry.Length);
                    using (var memoryStream = new MemoryStream(buffer))
                    {
                        LocrFiles.Add(LoadLocrFile(memoryStream, file, entry.Name, entry.FullName.Split('/')[0]));
                    }
                }
            }
        }

        public void LoadDlgeFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            LoadGameDataFolder(directory, groupByDirectories, categoryDirectory, (filePath, category) =>
            {
                var dlgeFile = new DlgeTextFile
                {
                    Name = Path.GetFileName(filePath),
                    Category = category
                };

                using (var fileStream = File.Open(filePath, FileMode.Open))
                {
                    var fileNameToSave = Path.GetFileName(filePath);

                    DlgeFiles.Add(LoadDlgeFile(fileStream, dlgeFile));
                }
            });
        }

        public void LoadDlgeFromZip(ZipArchive zipArchive, bool groupByDirectories, string categoryDirectory)
        {
            if (!groupByDirectories)
                throw new NotSupportedException();

            var entries = zipArchive.Entries.Where(e => e.FullName.Contains($"/{categoryDirectory}/") && !e.FullName.EndsWith($"/"));
            foreach (var entry in entries)
            {
                var category = entry.FullName.Split('/')[0];
                var dlgeFile = new DlgeTextFile
                {
                    Name = entry.Name,
                    Category = category
                };

                using (var stream = entry.Open())
                {
                    var buffer = new byte[entry.Length];
                    stream.Read(buffer, 0, (int)entry.Length);

                    using (var memoryStream = new MemoryStream(buffer))
                    {
                        DlgeFiles.Add(LoadDlgeFile(memoryStream, dlgeFile));
                    }
                }
            }
        }

        public void LoadRtlvFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            LoadGameDataFolder(directory, groupByDirectories, categoryDirectory, (filePath, category) =>
            {
                using (var inputStream = File.Open(filePath, FileMode.Open))
                {
                    var fileNameToSave = Path.GetFileName(filePath);

                    RtlvFiles.Add(LoadRtlvFile(inputStream, fileNameToSave, category));
                }
            });
        }

        public void LoadRtlvFromZip(ZipArchive zipArchive, bool groupByDirectories, string categoryDirectory)
        {
            if (!groupByDirectories)
                throw new NotSupportedException();

            var entries = zipArchive.Entries.Where(e => e.FullName.Contains($"/{categoryDirectory}/") && !e.FullName.EndsWith($"/"));

            foreach (var entry in entries)
            {
                var category = entry.FullName.Split('/')[0];
                using (var stream = entry.Open())
                {
                    var buffer = new byte[entry.Length];
                    stream.Read(buffer, 0, (int)entry.Length);
                    using (var memoryStream = new MemoryStream(buffer))
                    {
                        RtlvFiles.Add(LoadRtlvFile(memoryStream, entry.Name, category));
                    }
                }
            }
        }

        public LocrTextFile LoadLocrFile(Stream inputStream, LocrTextFile file, string fileNameToSave, string category)
        {
            using (var reader = new GlacierLocrBinaryReader(inputStream, _version, file))
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

                file.Name = fileNameToSave;
                file.Category = category;
                file.LanguageSections = languageSections;
                file.CypherStrategy = reader.CypherStrategy;

                return file;
            }
        }

        public DlgeTextFile LoadDlgeFile(Stream inputStream, DlgeTextFile dlgeFile)
        {
            var numberOfLanguages = 0;
            switch (_version)
            {
                case HitmanVersion.Version1:
                case HitmanVersion.Version1Epic:
                    numberOfLanguages = 12;
                    break;
                case HitmanVersion.Version2:
                    numberOfLanguages = 13;
                    break;
                case HitmanVersion.Version3:
                    numberOfLanguages = 5;
                    break;
            }

            var hasAnyStrings = false;
            var firstEmpty = false;

            using (var reader = new GlacierDlgeBinaryReader(inputStream, _version, numberOfLanguages))
            {
                ICypherStrategy cypherStrategy = new CypherStrategyTEA();
                reader.ReadHeader();
                int iteration = 0;

                while (reader.HasText())
                {
                    var structure = reader.ReadStructure(iteration, dlgeFile.Structures.LastOrDefault()?.MetaDataNegative ?? false);
                    structure.ChainId = DlgeFiles.Count;
                    int nonEmptyStrings = 0;

                    for (var i = 0; i < numberOfLanguages; i++)
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
                    else
                    {
                        if (iteration == 0)
                        {
                            firstEmpty = true;
                        }
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

        public RtlvTextFile LoadRtlvFile(Stream inputStream, string fileNameToSave, string category)
        {
            RtlvTextFile file;

            using (var reader = new GlacierRtlvBinaryReader(inputStream, _version))
            {
                file = reader.ReadFile();
                file.Name = fileNameToSave;
                file.Extra = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                file.Category = category;
            }

            return file;
        }

        public void LoadTextFolder(string directory)
        {
            var slovakContent = File.ReadAllLines(directory + @"\Slovak.txt", Encoding.Unicode).ToList();
            var englishContent = File.ReadAllLines(directory + @"\English.txt", Encoding.Unicode).ToList();

            if (File.Exists($@"{directory}\Extra.txt"))
            {
                var extra = File.ReadAllLines($@"{directory}\Extra.txt", Encoding.Unicode).ToList();

                foreach (var entry in extra)
                {
                    var split = entry.Split('\t');

                    if (split.Length == 3)
                    {
                        slovakContent.Add($"{split[0]}\t{split[1]}");
                        englishContent.Add($"{split[0]}\t{split[2]}");
                    }
                }
            }

            for (var i = 0; i < slovakContent.Count; i++)
            {
                var slovakLine = slovakContent[i];
                var englishLine = englishContent[i];

                string[] slovakTokens = slovakLine.Split(new[] { '\t' });
                string[] englishTokens = englishLine.Split(new[] { '\t' });

                if (slovakTokens.Length >= 2)
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
                    if (Categories.Count > 0)
                    {
                        foreach (var category in Categories.OrderBy(c => c.Length).ThenBy(c => c))
                        {
                            writer.WriteLine($"[{category}]");

                            foreach (var textEntry in
                                LocrFiles
                                    .Where(file => file.Category == category && file.LanguageSections.Count > languagePair.Value.Item1)
                                    .SelectMany(_ => _.LanguageSections[languagePair.Value.Item1].Entries
                                        .Select(text => new
                                        {
                                            Identifier = text.Hash,
                                            Entry = text.Entry,
                                            Extra = string.Empty
                                        }))
                                    .Concat(DlgeFiles
                                        .Where(file => file.Category == category)
                                        .SelectMany(_ => _.Structures)
                                        .GroupBy(s => s.Identifier)
                                        .Select(s => new
                                        {
                                            Identifier = s.Last().Identifier,
                                            Entry = s.Last().Dialogues[languagePair.Value.Item2],
                                            Extra = s.Last().ChainId.ToString()
                                        }))
                                    .Concat(RtlvFiles.Where(_ => _ != null && _.Category == category)
                                        .Select(_ => new
                                        {
                                            Identifier = _.Identifier,
                                            Entry = _.Sections[languagePair.Value.Item1].Lines.First(),
                                            Extra = string.Empty
                                        }))
                                    .Where(entry => entry.Entry != null)
                                    .GroupBy(entry => entry.Entry))
                            {
                                var id = string.Join(",", textEntry.Select(entry => string.Format("{0:X8}", entry.Identifier)).Distinct());
                                var value = textEntry.Key.Replace("\r\n", "\\n");
                                var extra = textEntry.Last().Extra;
                                writer.WriteLine($"{id}\t{value}\t{extra}");
                            }
                        }
                    }
                    else
                    {
                        foreach (var textEntry in
                            LocrFiles
                                .Where(file => file.LanguageSections.Count > languagePair.Value.Item1)
                                .SelectMany(_ => _.LanguageSections[languagePair.Value.Item1].Entries
                                    .Select(text => new
                                    {
                                        Identifier = text.Hash,
                                        Entry = text.Entry,
                                        Extra = string.Empty
                                    }))
                                .Concat(DlgeFiles
                                    .SelectMany(_ => _.Structures)
                                    .GroupBy(s => s.Identifier)
                                    .Select(s => new
                                    {
                                        Identifier = s.Last().Identifier,
                                        Entry = s.Last().Dialogues[languagePair.Value.Item2],
                                        Extra = s.Last().ChainId.ToString()
                                    }))
                                .Concat(RtlvFiles.Where(_ => _ != null)
                                    .Select(_ => new
                                    {
                                        Identifier = _.Identifier,
                                        Entry = _.Sections[languagePair.Value.Item1].Lines.First(),
                                        Extra = string.Empty
                                    }))
                                .Where(entry => entry.Entry != null)
                                .GroupBy(entry => entry.Entry))
                        {
                            var id = string.Join(",", textEntry.Select(entry => string.Format("{0:X8}", entry.Identifier)).Distinct());
                            var value = textEntry.Key.Replace("\r\n", "\\n");
                            var extra = textEntry.Last().Extra;
                            writer.WriteLine($"{id}\t{value}\t{extra}");
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
                foreach (var category in Categories)
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
                {
                    WriteLocresFile(fileHandle, file);
                }
            }
        }

        public void WriteLocrToZip(ZipArchive zipArchive, bool groupByDirectories, string categoryDirectory)
        {
            if (LocrFiles == null) return;

            if (!groupByDirectories)
            {
                throw new NotSupportedException();
            }

            foreach (var file in LocrFiles)
            {
                var path = $@"{file.Category}/{categoryDirectory}/{file.Name}";

                var entry = zipArchive.CreateEntry(path);
                using (var seekableMemoryStream = new MemoryStream())
                using (var stream = entry.Open())
                {
                    WriteLocresFile(seekableMemoryStream, file, true);
                    seekableMemoryStream.Seek(0, SeekOrigin.Begin);
                    seekableMemoryStream.CopyTo(stream);
                }
            }
        }

        private void WriteLocresFile(Stream outputStream, LocrTextFile file, bool leaveOpen = false)
        {
            using (var writer = new GlacierLocrBinaryWriter(outputStream, _version, file.CypherStrategy, leaveOpen))
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

        public void WriteDlgeFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            if (DlgeFiles == null) return;

            if (groupByDirectories)
            {
                foreach (var category in Categories)
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
                    WriteDlgeFile(file, dlgeFile);
                }
            }
        }

        public void WriteDlgeToZip(ZipArchive zipArchive, bool groupByDirectories, string categoryDirectory)
        {
            if (DlgeFiles == null) return;

            if (!groupByDirectories)
            {
                throw new NotSupportedException();
            }

            foreach (var dlgeFile in DlgeFiles)
            {
                var path = $@"{dlgeFile.Category}/{categoryDirectory}/{dlgeFile.Name}";

                var entry = zipArchive.CreateEntry(path);
                using (var seekableMemoryStream = new MemoryStream())
                using (var stream = entry.Open())
                {
                    WriteDlgeFile(seekableMemoryStream, dlgeFile, true);
                    seekableMemoryStream.Seek(0, SeekOrigin.Begin);
                    seekableMemoryStream.CopyTo(stream);
                }
            }
        }

        private void WriteDlgeFile(Stream outputStream, DlgeTextFile dlgeFile, bool leaveOpen = false)
        {
            using (var writer = new GlacierDlgeBinaryWriter(outputStream, _version, leaveOpen))
            {
                writer.WriteHeader();

                for (var iteration = 0; iteration < dlgeFile.Structures.Count; iteration++)
                {
                    var structure = dlgeFile.Structures[iteration];
                    var previousMetadataNegative = iteration != 0 && dlgeFile.Structures[iteration - 1].MetaDataNegative;
                    writer.Write(structure, iteration, previousMetadataNegative);

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

        public void WriteRtlvFolder(string directory, bool groupByDirectories, string categoryDirectory)
        {
            if (RtlvFiles == null) return;

            if (groupByDirectories)
            {
                foreach (var category in Categories)
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
                {
                    WriteRtlvFile(fileHandle, file);
                }
            }
        }

        public void WriteRtlvToZip(ZipArchive zipArchive, bool groupByDirectories, string categoryDirectory)
        {
            if (RtlvFiles == null) return;

            if (!groupByDirectories)
            {
                throw new NotSupportedException();
            }

            foreach (var file in RtlvFiles)
            {
                var path = $@"{file.Category}/{categoryDirectory}/{file.Name}";

                var entry = zipArchive.CreateEntry(path);
                using (var seekableMemoryStream = new MemoryStream())
                using (var stream = entry.Open())
                {
                    WriteRtlvFile(seekableMemoryStream, file, true);
                    seekableMemoryStream.Seek(0, SeekOrigin.Begin);
                    seekableMemoryStream.CopyTo(stream);
                }
            }
        }

        private void WriteRtlvFile(Stream outputStream, RtlvTextFile file, bool leaveOpen = false)
        {
            using (var writer = new GlacierRtlvBinaryWriter(outputStream, leaveOpen))
            {
                writer.Write(file);

                for (var i = 0; i < file.Sections.Count; i++)
                {
                    var offsetAtBeginningOfSection = writer.BaseStream.Position;
                    string finalString = GetReplacementString(file.Sections[i].Lines.First(), file.Identifier,
                        GetLanguageMap()["English"].Item1 == i);
                    writer.Write(finalString);
                    var offsetAtEndOfSection = writer.BaseStream.Position;
                    var sectionLength = offsetAtEndOfSection - offsetAtBeginningOfSection - 4;

                    if ((_version == HitmanVersion.Version2 || _version == HitmanVersion.Version3) && sectionLength == 4)
                    {
                        sectionLength = 0;
                    }

                    file.Sections[i].StartingOffset = (int) offsetAtBeginningOfSection - 12;
                    file.Sections[i].SectionLength = (short) sectionLength;
                }

                file.FileSize = (UInt32) writer.BaseStream.Position;

                writer.BaseStream.Seek(0, SeekOrigin.Begin);
                writer.Write(file);

                writer.BaseStream.Seek(0, SeekOrigin.End);
                writer.Write(file.Extra);
            }
        }

        public Dictionary<string, Tuple<int, int>> GetLanguageMap()
        {
            switch (_version)
            {
                case HitmanVersion.Version1:
                case HitmanVersion.Version2:
                    return new Dictionary<string, Tuple<int, int>>
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
                case HitmanVersion.Version1Epic:
                    return new Dictionary<string, Tuple<int, int>>
                    {
                        { "Empty", new Tuple<int, int>(0, 0) },
                        { "English", new Tuple<int, int>(1, 1) },
                        { "French", new Tuple<int, int>(2, 2)},
                        { "Italian", new Tuple<int, int>(3, 3) },
                        { "German", new Tuple<int, int>(4, 4) },
                        { "Spanish", new Tuple<int, int>(5, 5) },
                        { "Russian", new Tuple<int, int>(6, 6) },
                        { "Mexican", new Tuple<int, int>(7, 7) },
                        { "Portugese", new Tuple<int, int>(8, 8) },
                        { "Polish", new Tuple<int, int>(9, 9) },
                        { "Japanese", new Tuple<int, int>(10, 10) },
                        { "Chinese", new Tuple<int, int>(0, 11) },
                    };
                case HitmanVersion.Version3:
                    return new Dictionary<string, Tuple<int, int>>
                    {
                        { "English", new Tuple<int, int>(1, 0) },
                        { "French", new Tuple<int, int>(2, 1 )},
                        { "Italian", new Tuple<int, int>(3, 2) },
                        { "German", new Tuple<int, int>(4, 3) },
                        { "Spanish", new Tuple<int, int>(5, 4) },
                    };
                default:
                    throw new ArgumentException();
            }
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
                        File.Delete(filePath);
                        Console.WriteLine(filePath);
                    }
                }
            }
            else
            {
                foreach (var subdirectory in Directory.GetDirectories(directory))
                {
                    var category = subdirectory.Split(new [] { directory + @"\" }, StringSplitOptions.RemoveEmptyEntries)[0];

                    if (!Categories.Contains(category))
                    {
                        Categories.Add(category);
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
