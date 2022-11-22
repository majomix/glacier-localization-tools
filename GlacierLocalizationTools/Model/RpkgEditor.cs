using System;
using System.Collections.Generic;
using System.IO;

namespace GlacierLocalizationTools.Model
{
    public class RpkgEditor
    {
        public RpkgFileStructure Archive { get; private set; }

        public void LoadRpkgFileStructure(RpkgBinaryReader reader)
        {
            var hashes = new List<string>(); //File.ReadAllLines(@"Hitman_Hashes.txt");
            var dictionary = new Dictionary<UInt64, string>();

            foreach (var hash in hashes)
            {
                var parts = hash.Split(' ');
                var hashedName = UInt64.Parse(parts[0]);
                var name = parts[1];
                dictionary.Add(hashedName, name);
            }

            Archive = reader.ReadHeader();

            for(int i = 0; i < Archive.NumberOfFiles; i++)
            {
                var entry = reader.ReadEntry();
                Archive.Entries.Add(entry);

                if (dictionary.ContainsKey(entry.Hash))
                {
                    entry.Name = dictionary[entry.Hash];
                }
            }

            for (int i = 0; i < Archive.NumberOfFiles; i++)
            {
                Archive.Entries[i].Info = reader.ReadEntryInfo();
            }
        }

        public void ExtractFile(string directory, RpkgEntry rpkgEntry, RpkgBinaryReader reader)
        {
            if (reader.BaseStream.Position != (long) rpkgEntry.Offset) reader.BaseStream.Seek((long) rpkgEntry.Offset, SeekOrigin.Begin);

            string compoundName;

            if (rpkgEntry.Name == null)
            {
                compoundName = directory + @"\" + rpkgEntry.Info.Signature + @"\" + rpkgEntry.Hash.ToString("X") + ".dat";
            }
            else
            {
                compoundName = directory + @"\" + rpkgEntry.Info.Signature + @"\" + rpkgEntry.Name;
                compoundName = compoundName.Substring(0, compoundName.Length > 235 ? 235 : compoundName.Length);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(compoundName));

            try
            {
                using (FileStream fileStream = File.Open(compoundName, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                    {
                        if (rpkgEntry.IsCompressed)
                        {
                            writer.Write(reader.ReadCompressedBytes(rpkgEntry.CompressedSize, rpkgEntry.Info.DecompressedDataSize));
                        }
                        else
                        {
                            writer.Write(reader.ReadBytes((int)rpkgEntry.Info.DecompressedDataSize));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SaveOriginalRpkgFileStructure(RpkgBinaryWriter writer)
        {
            writer.Write(Archive);

            for (int i = 0; i < Archive.NumberOfFiles; i++)
            {
                writer.Write(Archive.Entries[i]);
            }

            for (int i = 0; i < Archive.NumberOfFiles; i++)
            {
                writer.Write(Archive.Entries[i].Info);
            }
        }

        public void SaveRpkgFileStructure(RpkgBinaryWriter writer)
        {
            writer.Write(Archive);

            for (int i = 0; i < Archive.NumberOfFiles; i++)
            {
                writer.Write(Archive.Entries[i]);
            }

            Archive.ResourceTableOffset = (uint)writer.BaseStream.Position;

            for (int i = 0; i < Archive.NumberOfFiles; i++)
            {
                writer.Write(Archive.Entries[i].Info);
            }

            Archive.ResourceTableSize = (uint)writer.BaseStream.Position - Archive.ResourceTableOffset;
            Archive.ResourceTableOffset -= 20;
        }

        public void UpdateSavedRpkgFileStructure(RpkgBinaryWriter writer)
        {
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            SaveOriginalRpkgFileStructure(writer);
        }

        public void SaveDataEntry(RpkgBinaryReader reader, RpkgBinaryWriter writer, RpkgEntry rpkgEntry)
        {
            long originalOffset = (long)rpkgEntry.Offset;
            rpkgEntry.Offset = (ulong)writer.BaseStream.Position;

            if (rpkgEntry.Import != null)
            {
                ImportNewFile(writer, rpkgEntry);
            }
            else
            {
                if (reader.BaseStream.Position != originalOffset) reader.BaseStream.Seek(originalOffset, SeekOrigin.Begin);
                int size = rpkgEntry.IsCompressed ? (int)rpkgEntry.CompressedSize : (int)rpkgEntry.Info.DecompressedDataSize;
                writer.Write(reader.ReadBytes(size));
            }
        }

        public void AppendDataEntry(RpkgBinaryWriter writer, RpkgEntry rpkgEntry)
        {
            writer.BaseStream.Seek(0, SeekOrigin.End);
            rpkgEntry.Offset = (ulong)writer.BaseStream.Position;
            ImportNewFile(writer, rpkgEntry);
        }

        public void ImportNewFile(RpkgBinaryWriter writer, RpkgEntry rpkgEntry)
        {
            var rawData = rpkgEntry.ImportRawData ?? File.ReadAllBytes(rpkgEntry.Import);

            if (rpkgEntry.IsCompressed)
            {
                rpkgEntry.CompressedSize = writer.WriteCompressedBytes(rawData);
            }
            else
            {
                writer.Write(rawData);
            }
        }

        public void InitializeHeader(List<RpkgEntry> entries)
        {
            Archive = new RpkgFileStructure
            {
                Signature = "GKPR",
                Entries = entries,
                NumberOfFiles = (uint) entries.Count
            };
        }
    }
}
