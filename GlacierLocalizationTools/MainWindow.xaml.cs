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

            using (FileStream fileStream = File.Open(@"H:\Steam Games\steamapps\common\Hitman™\Runtime\chunk0patch1.rpkg", FileMode.Open))
            {
                RpkgBinaryReader reader = new RpkgBinaryReader(1, fileStream);
                editor.LoadRpgkFileStructure(reader);

                for (int i = 0; i < editor.Archive.Entries.Count; i++ )
                {
                    try
                    {
                        editor.ExtractFile(@"H:\Steam Games\steamapps\common\Hitman™\Runtime\c0p1", editor.Archive.Entries[i], reader);
                    }
                    catch (Exception e) { }
                }
            }
        }
    }
}
