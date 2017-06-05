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
            TextConverter converter = new TextConverter();
            //converter.LoadTextFile(@"D:\Hitman data\chunk0_LOCR\0000000000001c7a.dat", new FileVersionSpecificationsBase());
            converter.LoadTextFile(@"D:\Hitman data\chunk0patch1_LOCR\0000000000000a3a.dat", new FileVersionSpecificationsUpdate());
        }

        private void xxtea(string[] args)
        {
            uint[] encryptionKeys = new uint[]
		    {
			    1397913399u,
			    1963346334u,
			    3174674147u,
			    2778624616u
		    };
            uint delta = 2654435769u;
            FileStream fileStream2 = new FileStream(args[0], FileMode.Open);
            BinaryReader binaryReader2 = new BinaryReader(fileStream2);
            StreamWriter streamWriter = new StreamWriter(Path.GetFileNameWithoutExtension(args[0]) + ".txt", false, Encoding.UTF8);
            int num10 = binaryReader2.ReadInt32() / 4;
            fileStream2.Seek((long)(num10 * 4), SeekOrigin.Begin);
            streamWriter.WriteLine(num10.ToString());
            for (int i = 0; i < num10; i++)
            {
                int numberOfEntries = binaryReader2.ReadInt32();
                streamWriter.WriteLine(numberOfEntries.ToString());
                for (int j = 0; j < numberOfEntries; j++)
                {
                    streamWriter.WriteLine(binaryReader2.ReadUInt32());
                    int stringLength = binaryReader2.ReadInt32();
                    byte[] outputArray = new byte[stringLength];
                    MemoryStream output = new MemoryStream(outputArray);
                    BinaryWriter binaryWriter2 = new BinaryWriter(output);
                    for (int l = 0; l < stringLength / 8; l++)
                    {
                        uint sum = 3337565984u;
                        uint firstWord = binaryReader2.ReadUInt32();
                        uint secondWord = binaryReader2.ReadUInt32();
                        for (uint num14 = 0u; num14 < 32u; num14 += 1u)
                        {
                            secondWord -= ((firstWord << 4 ^ firstWord >> 5) + firstWord ^ sum + encryptionKeys[(int)((UIntPtr)(sum >> 11 & 3))]);
                            sum -= delta;
                            firstWord -= ((secondWord << 4 ^ secondWord >> 5) + secondWord ^ sum + encryptionKeys[(int)((UIntPtr)(sum & 3))]);
                        }
                        binaryWriter2.Write(firstWord);
                        binaryWriter2.Write(secondWord);
                    }
                    string text = new string(Encoding.GetEncoding(65001).GetChars(outputArray));
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
