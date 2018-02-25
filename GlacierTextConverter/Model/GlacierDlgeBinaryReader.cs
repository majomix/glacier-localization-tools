﻿using GlacierTextConverter.Model;
using System;
using System.IO;
using System.Text;

namespace GlacierTextConverter
{
    public class GlacierDlgeBinaryReader : BinaryReader
    {
        public GlacierDlgeBinaryReader(FileStream fileStream, Encoding encoding)
            : base(fileStream, encoding)
        {
        }

        public string ReadString(ICypherStrategy cypherStrategy)
        {
            uint length = ReadUInt32();
            byte[] bytes = ReadBytes((int)length);
            return cypherStrategy.Decypher(bytes);
        }

        public bool ReadLanguageMetadataAndDetermineIfEmpty(int i)
        {
            ConfirmEquality(ReadInt32(), 0);

            if(i == 9)
            {
                ConfirmEquality(ReadInt32(), 4);
                ConfirmEquality(ReadInt32(), 5);

                return true;
            }
            else
            {
                ConfirmEquality(ReadInt32(), -1);
                ConfirmEquality(ReadInt32(), -1);

                return false;
            }
        }

        public DlgeStructure ReadStructure()
        {
            DlgeStructure structure = new DlgeStructure();

            ConfirmEquality(ReadInt32(), 0);
            ConfirmEquality(ReadInt32(), 1);
            ConfirmEquality(ReadByte(), 1);
            structure.Category = ReadInt32();
            structure.Identifier = ReadUInt32();
            ConfirmEquality(ReadInt32(), 0);
            ConfirmEquality(ReadInt64(), -1);
            ConfirmEquality(ReadInt64(), 0);
            ConfirmEquality(ReadInt32(), 2);
            ConfirmEquality(ReadInt32(), 3);

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
    }
}
