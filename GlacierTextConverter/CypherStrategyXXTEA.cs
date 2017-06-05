using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierTextConverter
{
    public class CypherStrategyXXTEA : ICypherStrategy
    {
        private int RunSymmetricCypher(uint firstWord, uint secondWord)
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

            return 226;
        }

        public string Decypher(byte[] input)
        {
            throw new NotImplementedException();
        }
    }
}
