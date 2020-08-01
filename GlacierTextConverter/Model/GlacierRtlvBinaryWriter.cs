using System;
using System.IO;
using System.Text;

namespace GlacierTextConverter.Model
{
    public class GlacierRtlvBinaryWriter : BinaryWriter
    {
        private ICypherStrategy myStrategy;

        public GlacierRtlvBinaryWriter(FileStream fileStream)
            : base(fileStream)
        {
            myStrategy = new CypherStrategyTEA();
        }

        public void Write(RtlvTextFile file)
        {
            Write(file.Header);
            this.WriteUInt32BE((UInt32)(file.FileSize - 16));
            Write(file.StaticContext);
            Write(file.Identifier);
            Write(file.StaticContext2);
            Write(file.Sections.Count);

            for (var i = 0; i < file.Sections.Count; i++)
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

            if (cypheredString.Length == 0)
            {
                Write((UInt32)0);
            }
            else
            {
                Write(cypheredString);
            }
        }
    }
}
