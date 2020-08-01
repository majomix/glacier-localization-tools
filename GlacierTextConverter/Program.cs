using GlacierTextConverter.Model;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GlacierTextConverter
{
    public enum HitmanVersion
    {
        Version1,
        Version2
    }

    public class Program
    {
        static void Main(string[] args)
        {
            bool export = true;
            var separateDirectories = false;
            string sourceDirectory = null;
            string directory = Directory.GetCurrentDirectory();
            var version = HitmanVersion.Version1;

            var tea = new CypherStrategyTEA();
            var perm = new CypherStrategyPermutation();

            OptionSet options = new OptionSet()
            .Add("import", value => export = false)
            .Add("dir=", value => directory = value)
            .Add("source=", value => sourceDirectory = value)
            .Add("separatedirs", value => separateDirectories = true)
            .Add("v2", value => version = HitmanVersion.Version2);

            options.Parse(Environment.GetCommandLineArgs());

            if (sourceDirectory == null)
            {
                sourceDirectory = directory;
            }

            TextConverter converter = new TextConverter(version);

            converter.LoadLocrFolder(sourceDirectory, separateDirectories, @"RCOL");
            converter.LoadDlgeFolder(sourceDirectory, separateDirectories, @"EGLD");
            converter.LoadRtlvFolder(sourceDirectory, separateDirectories, @"VLTR");

            if (export)
            {
                converter.WriteCombinedTextFile(directory);
            }
            else
            {
                converter.LoadTextFolder(directory);
                converter.WriteLocrFolder(directory, separateDirectories, @"RCOL");
                converter.WriteDlgeFolder(directory, separateDirectories, @"EGLD");
                converter.WriteRtlvFolder(directory, separateDirectories, @"VLTR");
            }
        }

        private void CompareContent(string entryPoint, string compareWith)
        {
            var list = new List<string>();
            foreach (var filePath in Directory.GetFiles(entryPoint, "*", SearchOption.AllDirectories))
            {
                var comparePath = filePath.Replace(entryPoint, compareWith);
                var original = File.ReadAllBytes(filePath);
                var rewritten = File.ReadAllBytes(comparePath);
                if (!original.SequenceEqual(rewritten))
                {
                    list.Add(comparePath);
                }
            }
        }
    }

}
