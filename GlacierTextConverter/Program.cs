using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GlacierTextConverter
{
    public class Program
    {
        static void Main(string[] args)
        {
            //byte[] input = new byte[] { 0xDC, 0xC5, 0x91, 0xC4, 0x96, 0x8A, 0xC5, 0x91, 0xDB, 0x9E, 0xC5, 0x96, 0xD4, 0x95 };

            //{
            //    StringBuilder sb = new StringBuilder();
            //    foreach (byte value in input)
            //    {
            //        int[] bits = new int[] { value & 1, (value & 2) >> 1, (value & 4) >> 2, (value & 8) >> 3, (value & 16) >> 4, (value & 32) >> 5, (value & 64) >> 6, (value & 128) >> 7 };
            //        int output = (bits[0] << 0) | (bits[1] << 4) | (bits[2] << 1) | (bits[3] << 5) | (bits[4] << 2) | (bits[5] << 6) | (bits[6] << 3) | (bits[7] << 7);
            //        int xored = output ^ 226;
            //        sb.Append(Char.ConvertFromUtf32(xored));
            //    }
            //    Console.WriteLine(sb.ToString());
            //}

            //{
            //    StringBuilder sb = new StringBuilder();
            //    foreach (byte value in input)
            //    {
            //        int[] bits = new int[] { value & 1, (value & 2) >> 1, (value & 4) >> 2, (value & 8) >> 3, (value & 16) >> 4, (value & 32) >> 5, (value & 64) >> 6, (value & 128) >> 7 };
            //        int output = (bits[0] << 0) | (bits[1] << 4) | (bits[2] << 1) | (bits[3] << 5) | (bits[4] << 2) | (bits[5] << 7) | (bits[6] << 3) | (bits[7] << 6);
            //        int xored = output ^ 34;
            //        sb.Append(Char.ConvertFromUtf32(xored));
            //    }
            //    Console.WriteLine(sb.ToString());
            //}

            //byte[] input = new byte[] { 0x43, 0x26, 0xDB, 0xFF, 0xCD, 0x33, 0x90, 0xB3 };

            //for (int level0 = 0; level0 < 8; level0++)
            //{
            //    Console.WriteLine(DateTime.Now.ToString("h:mm:ss tt"));
            //    for (int level1 = 0; level1 < 8; level1++)
            //    {
            //        for (int level2 = 0; level2 < 8; level2++)
            //        {
            //            for (int level3 = 0; level3 < 8; level3++)
            //            {
            //                for (int level4 = 0; level4 < 8; level4++)
            //                {
            //                    for (int level5 = 0; level5 < 8; level5++)
            //                    {
            //                        for (int level6 = 0; level6 < 8; level6++)
            //                        {
            //                            for (int level7 = 0; level7 < 8; level7++)
            //                            {
            //                                int[] levels = new int[] { level0, level1, level2, level3, level4, level5, level6, level7 };

            //                                if (levels.Distinct().Count() == 8)
            //                                {
            //                                    for (int xorkey = 0; xorkey < 256; xorkey++)
            //                                    {
            //                                        StringBuilder sb = new StringBuilder();
            //                                        foreach (byte value in input)
            //                                        {
            //                                            int[] bits = new int[] { value & 1, (value & 2) >> 1, (value & 4) >> 2, (value & 8) >> 3, (value & 16) >> 4, (value & 32) >> 5, (value & 64) >> 6, (value & 128) >> 7 };
            //                                            int output = (bits[0] << level0) | (bits[1] << level1) | (bits[2] << level2) | (bits[3] << level3) | (bits[4] << level4) | (bits[5] << level5) | (bits[6] << level6) | (bits[7] << level7);
            //                                            int xored = output ^ xorkey;
            //                                            sb.Append(Char.ConvertFromUtf32(xored));
            //                                        }
            //                                        Console.WriteLine(sb.ToString());
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //Console.WriteLine("Done: " + DateTime.Now.ToString("h:mm:ss tt"));

            //using (FileStream fileStream = File.Open(@"F:\C#\GlacierLocalizationTools\GlacierTextConverter\bin\Debug\out.txt", FileMode.Open))
            //{
            //    StreamReader reader = new StreamReader(fileStream);
            //    string line;
            //    while ((line = reader.ReadLine()) != null)
            //    {
            //        Regex rg = new Regex(@"^[a-zA-Z0-9,_]{8}$");
            //        if(rg.IsMatch(line))
            //        {
            //            Console.WriteLine(line);
            //        }
            //    }
            //}

            //byte[] input = new byte[] { 0x43, 0x26, 0xDB, 0xFF, 0xCD, 0x33, 0x90, 0xB3 };
            //byte[] input = new byte[] { 0xA1, 0x3A, 0x4D, 0x90, 0xAA, 0xC3, 0x15, 0xC2 };
            //byte[] input = new byte[] { 0x78, 0xAC, 0x15, 0x24, 0x28, 0xD5, 0x01, 0x25 };
            byte[] input_or = new byte[] { 0xBA, 0xA4, 0x13, 0x6D, 0x7D, 0xE8, 0x7D, 0x8D };
            byte[] input_iv = new byte[] { 0x6D, 0x13, 0xA4, 0xBA, 0x8D, 0x7D, 0xE8, 0x7D };
            byte[] input_or16 = new byte[] { 0x3F, 0x3F, 0x72, 0xEC, 0x79, 0x9A, 0xD2, 0x2A };
            byte[] input_iv16 = new byte[] { 0xEC, 0x72, 0x3F, 0x3F, 0x2A, 0xD2, 0x9A };
            uint[] keys = new uint[] { 0x53527737, 0x7506499E, 0xBD39AEE3, 0xA59E7268 };
            IEnumerable<byte> keysb = BitConverter.GetBytes(keys[0]).Concat(BitConverter.GetBytes(keys[1])).Concat(BitConverter.GetBytes(keys[2])).Concat(BitConverter.GetBytes(keys[3]));
            //byte[] xxtea2 = Xxtea2.XXTEA2.Decrypt(input, keysb.ToArray());
            byte[] xxtea1 = XXTea.Decrypt(Convert.ToBase64String(input_or16));
            for (int xorkey = 0; xorkey < 256; xorkey++)
            {
                StringBuilder sb = new StringBuilder();
                byte[] xxteaxored = new byte[8];
                for(int index = 0; index < 8; index++)
                {
                    xxteaxored[index] = (byte)(xxtea1[index] ^ xorkey);
                }
                string xxteastring = System.Text.Encoding.UTF8.GetString(xxteaxored, 0, xxteaxored.Length);
                Debug.WriteLine(xxteastring);
            }
        }


        private void xxtea(string[] args)
        {
            uint[] array5 = new uint[]
		    {
			    0x53527737,//1397913399u,
			    1963346334u,
			    3174674147u,
			    2778624616u
		    };
            uint num9 = 2654435769u;
            FileStream fileStream2 = new FileStream(args[0], FileMode.Open);
            BinaryReader binaryReader2 = new BinaryReader(fileStream2);
            StreamWriter streamWriter = new StreamWriter(Path.GetFileNameWithoutExtension(args[0]) + ".txt", false, Encoding.UTF8);
            int num10 = binaryReader2.ReadInt32() / 4;
            fileStream2.Seek((long)(num10 * 4), SeekOrigin.Begin);
            streamWriter.WriteLine(num10.ToString());
            for (int i = 0; i < num10; i++)
            {
                int num3 = binaryReader2.ReadInt32();
                streamWriter.WriteLine(num3.ToString());
                for (int j = 0; j < num3; j++)
                {
                    streamWriter.WriteLine(binaryReader2.ReadUInt32());
                    int num4 = binaryReader2.ReadInt32();
                    byte[] array3 = new byte[num4];
                    MemoryStream output = new MemoryStream(array3);
                    BinaryWriter binaryWriter2 = new BinaryWriter(output);
                    for (int l = 0; l < num4 / 8; l++)
                    {
                        uint num11 = 3337565984u;
                        uint num12 = binaryReader2.ReadUInt32();
                        uint num13 = binaryReader2.ReadUInt32();
                        for (uint num14 = 0u; num14 < 32u; num14 += 1u)
                        {
                            num13 -= ((num12 << 4 ^ num12 >> 5) + num12 ^ num11 + array5[(int)((UIntPtr)(num11 >> 11 & 3u))]);
                            num11 -= num9;
                            num12 -= ((num13 << 4 ^ num13 >> 5) + num13 ^ num11 + array5[(int)((UIntPtr)(num11 & 3u))]);
                        }
                        binaryWriter2.Write(num12);
                        binaryWriter2.Write(num13);
                    }
                    string text = new string(Encoding.GetEncoding(65001).GetChars(array3));
                    streamWriter.WriteLine(text);
                    binaryReader2.ReadByte();
                }
            }
            streamWriter.Close();
            binaryReader2.Close();
            fileStream2.Close();
        }
    }
}
