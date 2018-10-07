using GlacierTextConverter.Model;
using NDesk.Options;
using System;
using System.IO;

namespace GlacierTextConverter
{
    public class Program
    {
        static void Main(string[] args)
        {
            bool export = true;
            string directory = Directory.GetCurrentDirectory();

            OptionSet options = new OptionSet()
            .Add("import", value => export = false)
            .Add("dir=", value => directory = value);

            options.Parse(Environment.GetCommandLineArgs());

            TextConverter converter = new TextConverter();

            converter.LoadLocrFolder(directory + @"\RCOL");
            converter.LoadDlgeFolder(directory + @"\EGLD");
            converter.LoadRtlvFolder(directory + @"\VLTR");

            if (export)
            {
                converter.WriteCombinedTextFile(directory);
            }
            else
            {
                converter.LoadTextFolder(directory);
                converter.WriteLocrFolder(directory + @"\RCOL");
                converter.WriteDlgeFolder(directory + @"\EGLD");
                converter.WriteRtlvFolder(directory + @"\VLTR");
            }
        }
    }
}
