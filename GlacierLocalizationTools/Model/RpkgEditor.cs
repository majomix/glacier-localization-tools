using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierLocalizationTools.Model
{
    public class RpkgEditor
    {
        public RpkgFileStructure Archive { get; private set; }

        public void LoadRpgkFileStructure(RpkgBinaryReader reader)
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
                    writer.Write(reader.ReadCompressedBytes(rpkgEntry.CompressedSize, rpkgEntry.Info.DecompressedDataSize));
                    if(rpkgEntry.IsCompressed)
                    {
                        //writer.Write(reader.ReadBytes(rpkgEntry.CompressedSize));
                    }
                    else
                    {
                        //writer.Write(reader.ReadBytes((int)rpkgEntry.Info.DecompressedDataSize));
                    }
                }
            }
        }
    }
}
