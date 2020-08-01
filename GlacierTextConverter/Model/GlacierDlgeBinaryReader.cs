using GlacierTextConverter.Model;
using System;
using System.IO;
using System.Text;

namespace GlacierTextConverter
{
    public class GlacierDlgeBinaryReader : BinaryReader
    {
        private readonly HitmanVersion _version;

        public GlacierDlgeBinaryReader(FileStream fileStream, HitmanVersion version)
            : base(fileStream)
        {
            _version = version;
        }

        public string ReadString(ICypherStrategy cypherStrategy)
        {
            uint length = ReadUInt32();
            byte[] bytes = ReadBytes((int)length);
            return cypherStrategy.Decypher(bytes);
        }

        public bool ReadLanguageMetadataAndDetermineIfEmpty(int i, int iteration, DlgeStructure structure)
        {
            if (_version == HitmanVersion.Version1)
            {
                ConfirmEquality(ReadInt32(), 0);
            }

            if (i == 9)
            {
                var first = ReadInt32();
                var second = ReadInt32();
                if (first == -1 && second == -1)
                {
                    structure.MetaDataNegative = true;
                }
                else if ((first == 4 + iteration * 4) && (second == 5 + iteration * 4))
                {
                    structure.MetaDataNegative = false;
                }
                else
                {
                    throw new InvalidDataException();
                }

                return true;
            }
            
            ConfirmEquality(ReadInt32(), -1);
            ConfirmEquality(ReadInt32(), -1);

            return false;
        }

        public bool ReadHeader()
        {
            ConfirmEquality(ReadInt32(), 0);
            ConfirmEquality(ReadInt32(), 1);

            return true;
        }

        public DlgeStructure ReadStructure(int iteration)
        {
            DlgeStructure structure = new DlgeStructure();

            structure.Category = ReadInt32();
            structure.Identifier = ReadUInt32();
            ConfirmEquality(ReadInt32(), 0);
            ConfirmEquality(ReadInt64(), -1);

            ConfirmEquality(ReadInt32(), 0);

            if (_version == HitmanVersion.Version1)
            {
                ConfirmEquality(ReadInt32(), 0);
            }

            ConfirmEquality(ReadInt32(), 2 + iteration * 4);
            ConfirmEquality(ReadInt32(), 3 + iteration * 4);

            structure.Dialogues = new string[12];

            return structure;
        }

        private void ConfirmEquality(Int64 value, Int64 compared)
        {
            if(value != compared)
            {
                throw new InvalidDataException();
            }
        }

        public bool HasText()
        {
            return ReadByte() == 1;
        }
    }
}
