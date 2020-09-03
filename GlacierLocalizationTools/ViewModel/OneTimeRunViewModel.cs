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
        public ICommand ExtractByParameterCommand { get; }
        public ICommand ImportByParameterCommand { get; }

        public OneTimeRunViewModel()
        {
            ParseCommandLine();
            Model = new RpkgEditor();

            ImportByParameterCommand = new ImportByParameterCommand();
            ExtractByParameterCommand = new ExtractByParameterCommand();
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
                .Add("repack", value => Repack = true);

            options.Parse(Environment.GetCommandLineArgs());
        }

        public void Extract()
        {
            if (_targetDirectory != null && _sourceDirectory != null)
            {
                foreach(string file in Directory.GetFiles(_sourceDirectory, "*.rpkg"))
                {
                    LoadedFilePath = file;
                    LoadStructure();

                    Func<RpkgEntry, bool> function = entry => true;

                    if (TextsOnly)
                    {
                        function = entry => new[] { "FXFG", "EGLD", "RCOL", "VLTR" }.Contains(entry.Info.Signature);
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
                if (_targetDirectory != null && Directory.Exists(_targetDirectory))
                {
                    foreach (string file in Directory.GetFiles(_sourceDirectory, "*.rpkg"))
                    {
                        LoadedFilePath = file;
                        LoadStructure();
                        var filename = Path.GetFileNameWithoutExtension(file);
                        var split = filename.Split(new[] { @"patch" }, StringSplitOptions.None);

                        ResolveNewFiles(_targetDirectory, SeparateDirectories ? split[0] : null);

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
