using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierTextConverter
{
    public class GlacierBinaryReader : BinaryReader
    {
        private ICypherStrategy myStrategy;

        public GlacierBinaryReader(FileStream fileStream, Encoding encoding, ICypherStrategy cypherStrategy)
            : base(fileStream, encoding)
        {
            myStrategy = cypherStrategy;
        }

        public override string ReadString()
        {
            uint length = ReadUInt32();
            byte[] bytes = ReadBytes((int)length);
            byte zero = ReadByte(); // remove trailing zero
            return myStrategy.Decypher(bytes);
        }
    }
}
