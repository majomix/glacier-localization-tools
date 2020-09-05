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
            if (_version == HitmanVersion.Version1 || _version == HitmanVersion.Version1Epic)
            {
                ConfirmEquality(ReadInt32(), 0);
            }

            if (_version == HitmanVersion.Version1Epic && i == 0)
            {
                var multiplier = 6;

                var first = ReadInt32();
                var second = ReadInt32();

                if ((first == 4 + iteration * multiplier) && (second == 5 + iteration * multiplier))
                {
                    structure.MetaDataNegative = false;
                }
                else
                {
                    throw new InvalidDataException();
                }

                return false;
            }

            if (_version == HitmanVersion.Version1Epic && i == 10)
            {
                var first = ReadInt32();
                var second = ReadInt32();

                ConfirmEquality(first, 6 + iteration * 6);
                ConfirmEquality(second, 7 + iteration * 6);

                return false;
            }

            if (((_version == HitmanVersion.Version1 || _version == HitmanVersion.Version2) && i == 9))
            {
                var multiplier = _version == HitmanVersion.Version1Epic ? 6 : 4;

                var first = ReadInt32();
                var second = ReadInt32();
                if (first == -1 && second == -1)
                {
                    structure.MetaDataNegative = true;
                }
                else if ((first == 4 + iteration * multiplier) && (second == 5 + iteration * multiplier))
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

            var multiplier = _version == HitmanVersion.Version1Epic ? 6 : 4;

            if (_version == HitmanVersion.Version1 || _version == HitmanVersion.Version2)
            {
                ConfirmEquality(ReadInt64(), -1);
                ConfirmEquality(ReadInt32(), 0);
            }

            if (_version == HitmanVersion.Version1)
            {
                ConfirmEquality(ReadInt32(), 0);
            }

            ConfirmEquality(ReadInt32(), 2 + iteration * multiplier);
            ConfirmEquality(ReadInt32(), 3 + iteration * multiplier);

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
