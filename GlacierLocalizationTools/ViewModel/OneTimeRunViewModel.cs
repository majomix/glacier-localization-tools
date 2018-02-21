using GlacierLocalizationTools.Model;
using GlacierLocalizationTools.ViewModel.Commands;
using NDesk.Options;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace GlacierLocalizationTools.ViewModel
{
    internal class OneTimeRunViewModel : BaseViewModel
    {
        private string mySourceDirectory;
        private string myTargetDirectory;
        public bool? Export { get; set; }
        public bool TextsOnly { get; set; }
        public ICommand ExtractByParameterCommand { get; private set; }
        public ICommand ImportByParameterCommand { get; private set; }

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
                .Add("runtime=", value => mySourceDirectory = CreateFullPath(value, true))
                .Add("output=", value => myTargetDirectory = CreateFullPath(value, true))
                .Add("textsonly", value => TextsOnly = true);

            options.Parse(Environment.GetCommandLineArgs());
        }

        public void Extract()
        {
            if (myTargetDirectory != null && mySourceDirectory != null)
            {
                foreach(string file in Directory.GetFiles(mySourceDirectory, "*.rpkg"))
                {
                    LoadedFilePath = file;
                    LoadStructure();

                    Func<RpkgEntry, bool> function = entry => true;
                    if(TextsOnly)
                    {
                        function = entry => entry.Info.Signature == "EGLD" || entry.Info.Signature == "RCOL";
                    }

                    ExtractFile(myTargetDirectory, function);
                }
            }
        }

        public void Import()
        {
            if (myTargetDirectory != null && Directory.Exists(myTargetDirectory) && LoadedFilePath != null)
            {
                LoadStructure();
                ResolveNewFiles(myTargetDirectory);

                string randomName = LoadedFilePath + "_tmp" + new Random().Next().ToString();
                SaveStructure(randomName);

                File.Delete(LoadedFilePath);
                File.Move(randomName, LoadedFilePath);
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
