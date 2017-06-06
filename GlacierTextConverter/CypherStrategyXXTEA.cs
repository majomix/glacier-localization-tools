using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierTextConverter
{
    public class CypherStrategyXXTEA : ICypherStrategy
    {
        private void RunSymmetricCypher(uint firstWord, uint secondWord, BinaryWriter binaryWriter)
        {
            uint[] encryptionKeys = new uint[]
		    {
			    1397913399u,
			    1963346334u,
			    3174674147u,
			    2778624616u
		    };
            uint delta = 2654435769u;

            uint sum = 3337565984u;
            for (uint i = 0; i < 32; i++)
            {
                secondWord -= ((firstWord << 4 ^ firstWord >> 5) + firstWord ^ sum + encryptionKeys[(int)((UIntPtr)(sum >> 11 & 3))]);
                sum -= delta;
                firstWord -= ((secondWord << 4 ^ secondWord >> 5) + secondWord ^ sum + encryptionKeys[(int)((UIntPtr)(sum & 3))]);
            }

            binaryWriter.Write(firstWord);
            binaryWriter.Write(secondWord);
        }

        public string Decypher(byte[] input)
        {
            if(input.Length == 0)
            {
                return string.Empty;
            }

            byte[] outputArray = new byte[input.Length];
            MemoryStream output = new MemoryStream(outputArray);
            BinaryWriter myBinaryWriter = new BinaryWriter(output);

            for(int i = 0; i < input.Length; i += 8)
            {
                RunSymmetricCypher(CreateWord(input, i), CreateWord(input, i + 4), myBinaryWriter);
            }

            int zeroStrippedLength = 0;
            while (++zeroStrippedLength != input.Length && outputArray[zeroStrippedLength] != 0) ;
            byte[] zeroStrippedOutputArray = new byte[zeroStrippedLength];
            Array.Copy(outputArray, zeroStrippedOutputArray, zeroStrippedLength);
            return new string(Encoding.GetEncoding(65001).GetChars(zeroStrippedOutputArray));
        }

        private uint CreateWord(byte[] input, int start)
        {
            byte[] word = new byte[4];
            Array.Copy(input, start, word, 0, 4);
            return BitConverter.ToUInt32(word, 0);
        }
    }
}
