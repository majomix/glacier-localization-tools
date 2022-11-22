using GlacierLocalizationTools.Model;
using GlacierLocalizationTools.ViewModel.Commands;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GlacierLocalizationTools.ViewModel
{
    internal class OneTimeRunViewModel : BaseViewModel
    {
        private string _sourceDirectory;
        private string _targetDirectory;
        public bool? Export { get; set; }
        public bool TextsOnly { get; set; }
        public bool SeparateDirectories { get; set; }
        public bool Repack { get; set; }
        public bool Build { get; set; }
        public string ZipFile { get; set; }
        public ICommand ExtractByParameterCommand { get; }
        public ICommand ImportByParameterCommand { get; }
        public ICommand BuildByParameterCommand { get; }

        public OneTimeRunViewModel()
        {
            ParseCommandLine();
            Model = new RpkgEditor();

            ImportByParameterCommand = new ImportByParameterCommand();
            ExtractByParameterCommand = new ExtractByParameterCommand();
            BuildByParameterCommand = new BuildByParameterCommand();
        }

        public void ParseCommandLine()
        {
            OptionSet options = new OptionSet()
                .Add("export", value => Export = true)
                .Add("import", value => Export = false)
                .Add("runtime=", value => _sourceDirectory = CreateFullPath(value, true))
                .Add("userdata=", value => _targetDirectory = CreateFullPath(value, true))
                .Add("separatedirs", value => SeparateDirectories = true)
                .Add("textsonly", value => TextsOnly = true)
                .Add("repack", value => Repack = true)
                .Add("build", value =>
                {
                    Build = true;
                    Export = false;
                })
                .Add("zip=", value => ZipFile = CreateFullPath(value, false));

            options.Parse(Environment.GetCommandLineArgs());
        }

        public void Extract()
        {
            if (_targetDirectory != null && _sourceDirectory != null)
            {
                foreach (string file in Directory.GetFiles(_sourceDirectory, "*.rpkg"))
                {
                    LoadedFilePath = file;
                    LoadStructure();

                    Func<RpkgEntry, bool> function = entry => true;

                    if (TextsOnly)
                    {
                        function = entry => new[] { "EGLD", "RCOL", "VLTR" }.Contains(entry.Info.Signature);
                    }

                    string finalDirectory = _targetDirectory;
                    if (SeparateDirectories)
                    {
                        finalDirectory += @"\";
                        var filename = Path.GetFileNameWithoutExtension(file);
                        var split = filename.Split(new [] { @"patch" }, StringSplitOptions.None);
                        finalDirectory += split.Length == 1 ? filename : split[0];
;                   }
                    
                    ExtractFile(finalDirectory, function);
                }
            }
        }

        public void Import()
        {
            try
            {
                if (_sourceDirectory != null && (Directory.Exists(_targetDirectory) || ZipFile != null))
                {
                    foreach (string file in Directory.GetFiles(_sourceDirectory, "*.rpkg"))
                    {
                        LoadedFilePath = file;
                        LoadStructure();
                        var filename = Path.GetFileNameWithoutExtension(file);
                        var split = filename.Split(new[] { @"patch" }, StringSplitOptions.None);

                        if (ZipFile != null)
                        {
                            ResolveNewFilesFromZipFile(ZipFile, SeparateDirectories ? split[0] : null);
                        }
                        else
                        {
                            ResolveNewFilesFromDisk(_targetDirectory, SeparateDirectories ? split[0] : null);
                        }

                        if (Repack)
                        {
                            string randomName = LoadedFilePath + "_tmp" + new Random().Next().ToString();
                            SaveStructureByRepack(randomName);

                            File.Delete(LoadedFilePath);
                            File.Move(randomName, LoadedFilePath);
                        }
                        else
                        {
                            SaveStructureByAppend();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var mess = new List<string>();
                mess.Add(ex.Message);
                mess.Add(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    mess.Add(ex.InnerException.Message);
                    mess.Add(ex.InnerException.StackTrace);
                }

                MessageBox.Show(string.Join("\n", mess));
                throw;
            }
        }

        public void BuildPatch()
        {
            if (_sourceDirectory != null && (Directory.Exists(_targetDirectory) || ZipFile != null))
            {
                var groups = new Dictionary<string, List<string>>();
                var order = new List<string>();
                
                foreach (string file in Directory.GetFiles(_sourceDirectory, "*.rpkg"))
                {
                    var split = file.Split(new[] { $@"{_sourceDirectory}\", "patch", ".rpkg" }, StringSplitOptions.RemoveEmptyEntries);
                    var packageName = split[0];
                    order.Add(packageName);
                    if (!groups.ContainsKey(packageName))
                    {
                        groups[packageName] = new List<string>();
                    }
                    groups[packageName].Add(file);
                }

                foreach (var packageGroupName in order.Distinct())
                {
                    var packages = groups[packageGroupName];
                    var infoMap = CreateFileMapForPackageGroup(packages);
                    var entries = LoadZipContent(packageGroupName, infoMap, ZipFile);
                    SaveStructureByCreatingNewRpkg(packageGroupName, entries, _targetDirectory);
                }
            }
        }

        private string CreateFullPath(string path, bool isDirectory)
        {
            if (!String.IsNullOrEmpty(path) && !path.Contains(':'))
            {
                path = Directory.GetCurrentDirectory() + @"\" + path.Replace('/', '\\');
            }

            return (isDirectory || File.Exists(path)) ? path : null;
        }
    }
}
