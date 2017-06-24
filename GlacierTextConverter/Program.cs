using GlacierTextConverter.Model;
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
            //converter.LoadDatFile(@"D:\Hitman data\chunk0patch1_LOCR\0000000000000a3a.dat", new FileVersionSpecificationsUpdate());
            //converter.LoadDatFolder(@"D:\Hitman data\chunk0patch1_LOCR\", new FileVersionSpecificationsUpdate());
            //converter.WriteTextFile(@"F:\Hitman data\chunk0patch1_LOCR_txt\");
            //converter.LoadDatFolder(@"D:\Hitman data\dl0patch1patch_LOCR\", new FileVersionSpecificationsUpdate());
            //converter.WriteTextFile(@"F:\Hitman data\dl0patch1patch_LOCR_txt\");
            //converter.LoadDatFolder(@"D:\Hitman data\dlc0_LOCR\", new FileVersionSpecificationsBase());
            //converter.WriteTextFile(@"F:\Hitman data\dlc0_LOCR_txt\");
            //converter.LoadDatFolder(@"D:\Hitman data\chunk0_LOCR\", new FileVersionSpecificationsBase());
            //converter.WriteTextFile(@"F:\Hitman data\chunk0_LOCR_txt\");

            converter.LoadTextFolder(@"F:\Hitman data\chunk0patch1_LOCR_txt");
        }
    }
}
