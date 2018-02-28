using System.IO;

namespace GlacierLocalizationTools.Model
{
    public class RpkgEditor
    {
        public RpkgFileStructure Archive { get; private set; }

        public void LoadRpkgFileStructure(RpkgBinaryReader reader)
        {
            Archive = reader.ReadHeader();
            
            for(int i = 0; i < Archive.NumberOfFiles; i++)
            {
                Archive.Entries.Add(reader.ReadEntry());
            }

            for (int i = 0; i < Archive.NumberOfFiles; i++)
            {
                Archive.Entries[i].Info = reader.ReadEntryInfo();
            }
        }

        public void ExtractFile(string directory, RpkgEntry rpkgEntry, RpkgBinaryReader reader)
        {
            if (reader.BaseStream.Position != (long) rpkgEntry.Offset) reader.BaseStream.Seek((long) rpkgEntry.Offset, SeekOrigin.Begin);

            string compoundName = directory + @"\" + rpkgEntry.Info.Signature + @"\" + rpkgEntry.Hash.ToString("X") + ".dat";

            Directory.CreateDirectory(Path.GetDirectoryName(compoundName));

            if (File.Exists(compoundName))
            {
                compoundName += @"_" + Path.GetFileNameWithoutExtension(((FileStream)reader.BaseStream).Name);
            }

            using (FileStream fileStream = File.Open(compoundName, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    if(rpkgEntry.IsCompressed)
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

        public void UpdateSavedRpkgFileStructure(RpkgBinaryWriter writer)
        {
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            SaveOriginalRpkgFileStructure(writer);
        }

        public void SaveDataEntry(RpkgBinaryReader reader, RpkgBinaryWriter writer, RpkgEntry rpkgEntry)
        {
            long originalOffset = (long)rpkgEntry.Offset;
            rpkgEntry.Offset = (ulong)writer.BaseStream.Position;

            if(rpkgEntry.Import != null)
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

        private void ImportNewFile(RpkgBinaryWriter writer, RpkgEntry rpkgEntry)
        {
            using (BinaryReader importReader = new BinaryReader(File.Open(rpkgEntry.Import, FileMode.Open)))
            {
                if (rpkgEntry.IsCompressed)
                {
                    rpkgEntry.CompressedSize = writer.WriteCompressedBytes(importReader.ReadBytes((int)rpkgEntry.Info.DecompressedDataSize));
                }
                else
                {
                    writer.Write(importReader.ReadBytes((int)rpkgEntry.Info.DecompressedDataSize));
                }
            }
        }
    }
}
