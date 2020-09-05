using System;
using System.IO;

namespace GlacierTextConverter.Model
{
    public class GlacierDlgeBinaryWriter : BinaryWriter
    {
        private readonly HitmanVersion _version;
        private ICypherStrategy myStrategy;

        public GlacierDlgeBinaryWriter(FileStream fileStream, HitmanVersion version)
            : base(fileStream)
        {
            _version = version;
            myStrategy = new CypherStrategyTEA();
        }

        public override void Write(string value)
        {
            if (value == null) return;

            byte[] cypheredString = myStrategy.Cypher(value);

            Write(cypheredString.Length);
            Write(cypheredString);
        }

        public void WriteHeader()
        {
            Write((Int32)0);
            Write((Int32)1);
        }

        public void Write(DlgeStructure structure, int iteration)
        {
            var multiplier = _version == HitmanVersion.Version1Epic ? 6 : 4;

            Write((byte)1);
            Write(structure.Category);
            Write(structure.Identifier);
            Write((Int32)0);

            if (_version == HitmanVersion.Version1 || _version == HitmanVersion.Version2)
            {
                Write((Int64)(-1));
                Write((Int32)0);
            }

            if (_version == HitmanVersion.Version1)
            {
                Write((Int32)0);
            }

            Write((Int32)2 + iteration * multiplier);
            Write((Int32)3 + iteration * multiplier);
        }

        public void WriteTrailingBytes(int i, int iteration, DlgeStructure structure)
        {
            var multiplier = _version == HitmanVersion.Version1Epic ? 6 : 4;

            if (_version == HitmanVersion.Version1 || _version == HitmanVersion.Version1Epic)
            {
                Write((Int32)0);
            }

            if (_version == HitmanVersion.Version1Epic && i == 0)
            {
                Write((Int32)4 + iteration * multiplier);
                Write((Int32)5 + iteration * multiplier);
                return;
            }

            if (_version == HitmanVersion.Version1Epic && i == 10)
            {
                Write((Int32)6 + iteration * multiplier);
                Write((Int32)7 + iteration * multiplier);
                return;
            }

            if ((_version == HitmanVersion.Version1 || _version == HitmanVersion.Version2) && i == 9)
            {
                if (structure.MetaDataNegative)
                {
                    Write((Int32)(-1));
                    Write((Int32)(-1));
                }
                else
                {
                    Write((Int32)4 + iteration * 4);
                    Write((Int32)5 + iteration * 4);
                }

                return;
            }
            
            Write((Int32)(-1));
            Write((Int32)(-1));
        }
    }
}
