using GlacierTextMerger.Model;

namespace GlacierTextMerger
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] folders = new string[]
            {
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc0_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc0patch1_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc1_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc1patch1_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc2_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc2patch1_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc3_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc3patch1_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc4_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc4patch1_RCOL",
                //@"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc5_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc5patch1_RCOL",
                //@"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc6_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\dlc6patch1_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\chunk0_RCOL",
                @"H:\Steam Games\steamapps\common\Hitman™\Runtime\chunk0patch1_RCOL"
            };
            TextMerger merger = new TextMerger();
            merger.LoadTextFolders(folders);
            merger.WriteCompactedTexts(@"H:\Steam Games\steamapps\common\Hitman™\Runtime\texts");
        }
    }
}
