using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierLocalizationTools.Model
{
    public class RpkgBinaryReader : BinaryReader
    {
        private int myVersion;

        public RpkgBinaryReader(int version, FileStream fileStream)
            : base(fileStream)
        {
            myVersion = version;
        }

        public RpkgFileStructure ReadHeader()
        {
            RpkgFileStructure rpkgFile = new RpkgFileStructure();

            rpkgFile.Signature = new string(ReadChars(4));
            rpkgFile.NumberOfFiles = ReadUInt32();
            rpkgFile.ResourceTableOffset = ReadUInt32();
            rpkgFile.ResourceTableSize = ReadUInt32();

            if (myVersion == 0)
            {
                rpkgFile.BaseVersionWord = ReadUInt32();
                if(rpkgFile.BaseVersionWord != 0)
                {
                    BaseStream.Seek(-4, SeekOrigin.Current);
                }
            }
            else if (myVersion == 1)
            {
                uint numberOfLongs = ReadUInt32();
                for(int i = 0; i < numberOfLongs; i++)
                {
                    rpkgFile.PatchUnknownValues.Add(ReadUInt64());
                }
            }

            return rpkgFile;
        }

        public RpkgEntry ReadEntry()
        {
            RpkgEntry entry = new RpkgEntry();
            entry.Hash = ReadUInt64();
            entry.Offset = ReadUInt64();
            entry.Size = ReadUInt32();

            return entry;
        }
    }
}
