using System.IO;
using System.Text;

namespace GlacierTextConverter.Model
{
    public class GlacierLocrBinaryWriter : BinaryWriter
    {
        private readonly HitmanVersion _version;
        private ICypherStrategy myStrategy;

        public GlacierLocrBinaryWriter(Stream stream, HitmanVersion version, ICypherStrategy cypherStrategy, bool leaveOpen)
            : base(stream, Encoding.UTF8, leaveOpen)
        {
            _version = version;
            myStrategy = cypherStrategy;
        }

        public override void Write(string value)
        {
            byte[] cypheredString = myStrategy.Cypher(value);

            Write(cypheredString.Length);
            Write(cypheredString);
            Write((byte)0);
        }

        public void WriteHeader(LocrTextFile file)
        {
            if (file.HeaderValue != null)
            {
                Write(file.HeaderValue.Value);
            }
        }
    }
}
