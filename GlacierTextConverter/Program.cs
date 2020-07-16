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
            string sourceDirectory = null;
            string directory = Directory.GetCurrentDirectory();

            var tea = new CypherStrategyTEA();
            var perm = new CypherStrategyPermutation();

            OptionSet options = new OptionSet()
            .Add("import", value => export = false)
            .Add("dir=", value => directory = value)
            .Add("source=", value => sourceDirectory = value);

            options.Parse(Environment.GetCommandLineArgs());

            if (sourceDirectory == null)
            {
                sourceDirectory = directory;
            }

            TextConverter converter = new TextConverter();

            converter.LoadLocrFolder(sourceDirectory + @"\RCOL");
            converter.LoadDlgeFolder(sourceDirectory + @"\EGLD");
            converter.LoadRtlvFolder(sourceDirectory + @"\VLTR");

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
