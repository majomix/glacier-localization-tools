using System;
using System.IO;
using System.Text;

namespace GlacierTextConverter.Model
{
    public class GlacierDlgeBinaryWriter : BinaryWriter
    {
        private ICypherStrategy myStrategy;

        public GlacierDlgeBinaryWriter(FileStream fileStream, Encoding encoding)
            : base(fileStream, encoding)
        {
            myStrategy = new CypherStrategyTEA();
        }

        public override void Write(string value)
        {
            if (value == null) return;

            byte[] cypheredString = myStrategy.Cypher(value);

            Write(cypheredString.Length);
            Write(cypheredString);
        }

        public void Write(DlgeStructure structure)
        {
            Write((Int32)0);
            Write((Int32)1);
            Write((byte)1);
            Write(structure.Category);
            Write(structure.Identifier);
            Write((Int32)0);
            Write((Int64)(-1));
            Write((Int64)0);
            Write((Int32)2);
            Write((Int32)3);
        }

        public void WriteTrailingBytes(int index)
        {
            Write((Int32)0);

            if(index == 9)
            {
                Write((Int32)4);
                Write((Int32)5);
            }
            else
            {
                Write((Int32)(-1));
                Write((Int32)(-1));
            }
        }
    }
}
