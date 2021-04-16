using GlacierTextConverter.Model;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GlacierTextConverter
{
    public enum HitmanVersion
    {
        Version1,
        Version1Epic,
        Version2,
        Version3
    }

    public class Program
    {
        static void Main(string[] args)
        {
            bool export = true;
            var separateDirectories = false;
            string sourceDirectory = null;
            string sourceZip = null;
            string targetZip = null;
            string directory = Directory.GetCurrentDirectory();
            var version = HitmanVersion.Version1;

            var tea = new CypherStrategyTEA();
            var perm = new CypherStrategyPermutation();

            OptionSet options = new OptionSet()
            .Add("import", value => export = false)
            .Add("dir=", value => directory = value)
            .Add("source=", value => sourceDirectory = value)
            .Add("sourcezip=", value => sourceZip = value)
            .Add("targetzip=", value => targetZip = value)
            .Add("separatedirs", value => separateDirectories = true)
            .Add("v1e", value => version = HitmanVersion.Version1Epic)
            .Add("v2", value => version = HitmanVersion.Version2)
            .Add("v3", value => version = HitmanVersion.Version3);

            options.Parse(Environment.GetCommandLineArgs());

            if (sourceDirectory == null)
            {
                sourceDirectory = directory;
            }

            TextConverter converter = new TextConverter(version);

            if (sourceZip == null && Directory.Exists(sourceDirectory))
            {
                Console.WriteLine("Loading RCOL");
                converter.LoadLocrFolder(sourceDirectory, separateDirectories, @"RCOL");
                Console.WriteLine("Loading EGLD");
                converter.LoadDlgeFolder(sourceDirectory, separateDirectories, @"EGLD");
                Console.WriteLine("Loading VLTR");
                converter.LoadRtlvFolder(sourceDirectory, separateDirectories, @"VLTR");
            }
            else if (File.Exists(sourceZip))
            {
                var zipFile = File.ReadAllBytes(sourceZip);

                using (var inMemoryZip = new MemoryStream(zipFile))
                using (var zipArchive = new ZipArchive(inMemoryZip, ZipArchiveMode.Read))
                {
                    var folders = zipArchive.Entries.Select(e => e.FullName.Split('/')[0]).Distinct();
                    converter.Categories.AddRange(folders);

                    Console.WriteLine("Loading RCOL");
                    converter.LoadLocrFromZip(zipArchive, separateDirectories, @"RCOL");
                    Console.WriteLine("Loading EGLD");
                    converter.LoadDlgeFromZip(zipArchive, separateDirectories, @"EGLD");
                    Console.WriteLine("Loading VLTR");
                    converter.LoadRtlvFromZip(zipArchive, separateDirectories, @"VLTR");
                }
            }

            if (export)
            {
                converter.WriteCombinedTextFile(directory);
            }
            else
            {
                Console.WriteLine("Loading User Text");
                converter.LoadTextFolder(directory);

                if (targetZip != null)
                {
                    using (var inMemoryZip = new MemoryStream())
                    {
                        using (var zipArchive = new ZipArchive(inMemoryZip, ZipArchiveMode.Create, true))
                        {
                            Console.WriteLine("Writing RCOL");
                            converter.WriteLocrToZip(zipArchive, separateDirectories, @"RCOL");
                            Console.WriteLine("Writing EGLD");
                            converter.WriteDlgeToZip(zipArchive, separateDirectories, @"EGLD");
                            Console.WriteLine("Writing VLTR");
                            converter.WriteRtlvToZip(zipArchive, separateDirectories, @"VLTR");
                        }

                        using (var fileStream = new FileStream(targetZip, FileMode.Create))
                        {
                            inMemoryZip.Seek(0, SeekOrigin.Begin);
                            inMemoryZip.CopyTo(fileStream);
                        }
                    }

                }
                else if (Directory.Exists(directory))
                {
                    Console.WriteLine("Writing RCOL");
                    converter.WriteLocrFolder(directory, separateDirectories, @"RCOL");
                    Console.WriteLine("Writing EGLD");
                    converter.WriteDlgeFolder(directory, separateDirectories, @"EGLD");
                    Console.WriteLine("Writing VLTR");
                    converter.WriteRtlvFolder(directory, separateDirectories, @"VLTR");
                }
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
