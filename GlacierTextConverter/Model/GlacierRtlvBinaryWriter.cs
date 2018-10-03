using System;
using System.IO;
using System.Text;

namespace GlacierTextConverter.Model
{
    public class GlacierRtlvBinaryWriter : BinaryWriter
    {
        private ICypherStrategy myStrategy;

        public GlacierRtlvBinaryWriter(FileStream fileStream, Encoding encoding)
            : base(fileStream, encoding)
        {
            myStrategy = new CypherStrategyTEA();
        }

        public void Write(RtlvTextFile file)
        {
            Write(file.Header);
            Write(file.FileSize);
            Write(file.StaticContext);
            Write(file.Identifier);
            Write(file.StaticContext2);

            for (var i = 0; i < 12; i++)
            {
                Write(file.Sections[i].SectionLength);
                Write(file.Sections[i].Unknown);
                Write(file.Sections[i].Zeros);
                Write(file.Sections[i].StartingOffset);
                Write(file.Sections[i].Zeros2);
            }
        }

        public override void Write(string value)
        {
            byte[] cypheredString = myStrategy.Cypher(value);

            Write(cypheredString.Length);
            Write(cypheredString);
        }
    }
}
