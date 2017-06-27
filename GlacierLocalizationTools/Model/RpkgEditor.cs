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

        public void SaveRpkgFileStructure(RpkgBinaryWriter writer)
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

        public void SaveDataEntry(RpkgBinaryReader reader, RpkgBinaryWriter writer, RpkgEntry rpkgEntry)
        {
            if(rpkgEntry.Import != null)
            {
                using (FileStream importFileStream = File.Open(rpkgEntry.Import, FileMode.Open))
                {
                    BinaryReader importReader = new BinaryReader(importFileStream);
                    writer.WriteCompressedBytes(importReader.ReadBytes((int)rpkgEntry.Info.DecompressedDataSize));
                }
            }
            else
            {
                if (reader.BaseStream.Position != (long)rpkgEntry.Offset) reader.BaseStream.Seek((long)rpkgEntry.Offset, SeekOrigin.Begin);

                if (rpkgEntry.IsCompressed)
                {
                    writer.Write(reader.ReadBytes((int)rpkgEntry.CompressedSize));
                }
                else
                {
                    writer.Write(reader.ReadBytes((int)rpkgEntry.Info.DecompressedDataSize));
                }
            }
        }
    }
}
