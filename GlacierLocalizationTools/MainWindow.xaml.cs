using GlacierLocalizationTools.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GlacierRpkgEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            RpkgEditor editor = new RpkgEditor();

            using (FileStream fileStream = File.Open(@"F:\Hitman data\dlc0patch2.rpkg", FileMode.Open))
            {
                RpkgBinaryReader reader = new RpkgBinaryReader(1, fileStream);
                editor.LoadRpkgFileStructure(reader);

                ResolveNewFiles(@"F:\Hitman data\import", editor);

                using (FileStream outputFileStream = File.Open(@"F:\Hitman data\dlc0patch2_out.rpkg", FileMode.Create))
                {
                    using(RpkgBinaryWriter writer = new RpkgBinaryWriter(1, outputFileStream))
                    {
                        editor.SaveOriginalRpkgFileStructure(writer);

                        for (int i = 0; i < editor.Archive.Entries.Count; i++)
                        {
                            editor.SaveDataEntry(reader, writer, editor.Archive.Entries[i]);
                        }

                        editor.UpdateSavedRpkgFileStructure(writer);
                    }
                }

                //for (int i = 0; i < editor.Archive.Entries.Count; i++ )
                //{
                    //  editor.ExtractFile(@"H:\Steam Games\steamapps\common\Hitman™\Runtime\c0p1", editor.Archive.Entries[i], reader);
                //}
            }
        }

        public void ResolveNewFiles(string directory, RpkgEditor Model)
        {
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                string[] tokens = file.Split(new string[] { directory + @"\" }, StringSplitOptions.RemoveEmptyEntries);
                if (!String.IsNullOrWhiteSpace(tokens[0]))
                {
                    string[] filepath = tokens[0].Split('\\');
                    RpkgEntry currentEntry = Model.Archive.Entries.SingleOrDefault(_ => _.Info.Signature == filepath[0] && _.Hash == Convert.ToUInt64(filepath[1].Split(new string[] { ".dat" }, StringSplitOptions.None)[0], 16));

                    if (currentEntry != null)
                    {
                        currentEntry.Import = file;
                        currentEntry.Info.DecompressedDataSize = (uint)new FileInfo(file).Length;
                    }
                }
            }
        }
    }
}
