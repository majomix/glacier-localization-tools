using System.IO;
using System.Linq;

namespace GlacierTextConverter.Model
{
    public sealed class GlacierLocrBinaryReader : BinaryReader
    {
        public int NumberOfLanguages { get; }
        public ICypherStrategy CypherStrategy { get; private set; }

        public GlacierLocrBinaryReader(Stream stream, HitmanVersion version, LocrTextFile file)
            : base(stream)
        {
            switch (version)
            {
                case HitmanVersion.Version1:
                case HitmanVersion.Version1Epic:
                {
                    int initialOffset = ReadInt32();
                    NumberOfLanguages = initialOffset / 4;
                    SetDefaultStrategy();
                    stream.Seek(0, SeekOrigin.Begin);
                } 
                break;
                case HitmanVersion.Version2:
                case HitmanVersion.Version3:
                {
                    file.HeaderValue = ReadByte();
                    int initialOffset = ReadInt32();
                    NumberOfLanguages = (initialOffset - 1) / 4;
                    CypherStrategy = new CypherStrategyTEA();
                    stream.Seek(1, SeekOrigin.Begin);
                }
                break;
            }
        }

        public override string ReadString()
        {
            uint length = ReadUInt32();
            byte[] bytes = ReadBytes((int)length);
            byte zero = ReadByte(); // remove trailing zero
            string decyphered = CypherStrategy.Decypher(bytes);
            if(IsUsingIncorrectStrategy(bytes, decyphered))
            {
                CypherStrategy = new CypherStrategyTEA();
                decyphered = CypherStrategy.Decypher(bytes);
            }
            return decyphered;
        }

        private void SetDefaultStrategy()
        {
            switch(NumberOfLanguages)
            {
                case 10:
                    CypherStrategy = new CypherStrategyPermutation();
                    break;
                case 12:
                    CypherStrategy = new CypherStrategyTEA();
                    break;
                default:
                    throw new InvalidDataException("Unsupported number of languages");
            }
        }

        private bool IsUsingIncorrectStrategy(byte[] bytes, string output)
        {
            if (CypherStrategy is CypherStrategyPermutation && bytes.Length % 8 == 0)
            {
                if(output.Any(c => (c < 32 && c != 10 && c!= 13)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
