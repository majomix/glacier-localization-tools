using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierTextConverter.Model
{
    public class CypherStrategyPermutation : ICypherStrategy
    {
        private int RunSymmetricKeyDecryption(byte value)
        {
            int[] bits = new int[] { value & 1, (value & 2) >> 1, (value & 4) >> 2, (value & 8) >> 3, (value & 16) >> 4, (value & 32) >> 5, (value & 64) >> 6, (value & 128) >> 7 };
            int output = (bits[0] << 0) | (bits[1] << 4) | (bits[2] << 1) | (bits[3] << 5) | (bits[4] << 2) | (bits[5] << 6) | (bits[6] << 3) | (bits[7] << 7);
            return output ^ 226;
        }

        private int RunSymmetricKeyDecryption_AlternativeKeys(byte value)
        {
            int[] bits = new int[] { value & 1, (value & 2) >> 1, (value & 4) >> 2, (value & 8) >> 3, (value & 16) >> 4, (value & 32) >> 5, (value & 64) >> 6, (value & 128) >> 7 };
            int output = (bits[0] << 0) | (bits[1] << 4) | (bits[2] << 1) | (bits[3] << 5) | (bits[4] << 2) | (bits[5] << 7) | (bits[6] << 3) | (bits[7] << 6);
            return output ^ 34;
        }

        private int RunSymmetricKeyEncryption(byte value)
        {
            value ^= 226;
            return (value & 0x81) | (value & 2) << 1 | (value & 4) << 2 | (value & 8) << 3 | (value & 0x10) >> 3 | (value & 0x20) >> 2 | (value & 0x40) >> 1;
        }

        public String Decypher(byte[] input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<byte> outputBytes = new List<byte>();
            foreach (byte value in input)
            {
                outputBytes.Add((byte)RunSymmetricKeyDecryption(value));
            }
            stringBuilder.Append(Encoding.UTF8.GetString(outputBytes.ToArray()));
            return stringBuilder.ToString();
        }

        public void DiscoverCypherKeys(byte[] input)
        {
            for (int level0 = 0; level0 < 8; level0++)
            {
                Console.WriteLine(DateTime.Now.ToString("h:mm:ss tt"));
                for (int level1 = 0; level1 < 8; level1++)
                {
                    for (int level2 = 0; level2 < 8; level2++)
                    {
                        for (int level3 = 0; level3 < 8; level3++)
                        {
                            for (int level4 = 0; level4 < 8; level4++)
                            {
                                for (int level5 = 0; level5 < 8; level5++)
                                {
                                    for (int level6 = 0; level6 < 8; level6++)
                                    {
                                        for (int level7 = 0; level7 < 8; level7++)
                                        {
                                            int[] levels = new int[] { level0, level1, level2, level3, level4, level5, level6, level7 };

                                            if (levels.Distinct().Count() == 8)
                                            {
                                                for (int xorkey = 0; xorkey < 256; xorkey++)
                                                {
                                                    StringBuilder sb = new StringBuilder();
                                                    foreach (byte value in input)
                                                    {
                                                        int[] bits = new int[] { value & 1, (value & 2) >> 1, (value & 4) >> 2, (value & 8) >> 3, (value & 16) >> 4, (value & 32) >> 5, (value & 64) >> 6, (value & 128) >> 7 };
                                                        int output = (bits[0] << level0) | (bits[1] << level1) | (bits[2] << level2) | (bits[3] << level3) | (bits[4] << level4) | (bits[5] << level5) | (bits[6] << level6) | (bits[7] << level7);
                                                        int xored = output ^ xorkey;
                                                        sb.Append(Char.ConvertFromUtf32(xored));
                                                    }
                                                    Console.WriteLine(sb.ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Done: " + DateTime.Now.ToString("h:mm:ss tt"));
        }

        public byte[] Cypher(string input)
        {
            List<byte> outputBytes = new List<byte>();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            foreach (byte value in inputBytes)
            {
                outputBytes.Add((byte)RunSymmetricKeyEncryption(value));
            }

            return outputBytes.ToArray();
        }
    }
}
