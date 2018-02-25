using System;
using System.IO;
using System.Text;

namespace GlacierTextConverter.Model
{
    public class GlacierLocrBinaryWriter : BinaryWriter
    {
        private ICypherStrategy myStrategy;

        public GlacierLocrBinaryWriter(FileStream fileStream, Encoding encoding, ICypherStrategy cypherStrategy)
            : base(fileStream, encoding)
        {
            myStrategy = cypherStrategy;
        }

        public override void Write(string value)
        {
            byte[] cypheredString = myStrategy.Cypher(value);

            Write(cypheredString.Length);
            Write(cypheredString);
            Write((byte)0);
        }
    }
}
