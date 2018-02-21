using GlacierLocalizationTools.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace GlacierLocalizationTools.ViewModel
{
    internal abstract class BaseViewModel : INotifyPropertyChanged
    {
        private int myCurrentProgress = 100;
        private string myLoadedFilePath;
        private string myCurrentFile;
        private bool myHasError;

        public RpkgEditor Model { get; protected set; }
        public string LoadedFilePath
        {
            get { return myLoadedFilePath; }
            set
            {
                if (myLoadedFilePath != value)
                {
                    myLoadedFilePath = value;
                    OnPropertyChanged("LoadedFilePath");
                }
            }
        }
        public string CurrentFile
        {
            get { return myCurrentFile; }
            protected set
            {
                if (myCurrentFile != value)
                {
                    myCurrentFile = value;
                    OnPropertyChanged("CurrentFile");
                }
            }
        }
        public int CurrentProgress
        {
            get { return myCurrentProgress; }
            protected set
            {
                if (myCurrentProgress != value)
                {
                    myCurrentProgress = value;
                    OnPropertyChanged("CurrentProgress");
                }
            }
        }
        public bool HasError
        {
            get { return myHasError; }
            set
            {
                if (myHasError != value)
                {
                    myHasError = value;
                    OnPropertyChanged("HasError");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler RequestClose;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void OnRequestClose(EventArgs e)
        {
            RequestClose(this, e);
        }

        public void LoadStructure()
        {
            using (RpkgBinaryReader reader = new RpkgBinaryReader(GetRpkgVersion(LoadedFilePath), File.Open(LoadedFilePath, FileMode.Open)))
            {
                Model.LoadRpkgFileStructure(reader);
                OnPropertyChanged("Model");
            }
        }

        public void ExtractFile(string directory, Func<RpkgEntry, bool> function)
        {
            using (RpkgBinaryReader reader = new RpkgBinaryReader(GetRpkgVersion(LoadedFilePath), File.Open(LoadedFilePath, FileMode.Open)))
            {
                IEnumerable<RpkgEntry> rpkgEntries = Model.Archive.Entries.Where(function);
                long currentSize = 0;
                long totalSize = rpkgEntries.Sum(_ => _.CompressedSize);

                foreach (RpkgEntry entry in rpkgEntries)
                {
                    Model.ExtractFile(directory, entry, reader);
                    CurrentProgress = (int)(currentSize * 100.0 / totalSize);
                    CurrentFile = entry.Hash.ToString();
                    currentSize += entry.CompressedSize;
                }
            }
        }

        public void ResolveNewFiles(string directory)
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


        public void SaveStructure(string path)
        {
            using (RpkgBinaryReader reader = new RpkgBinaryReader(GetRpkgVersion(LoadedFilePath), File.Open(LoadedFilePath, FileMode.Open)))
            {
                using (RpkgBinaryWriter writer = new RpkgBinaryWriter(GetRpkgVersion(path), File.Open(path, FileMode.Create)))
                {
                    Model.SaveOriginalRpkgFileStructure(writer);

                    foreach (RpkgEntry entry in Model.Archive.Entries)
                    {
                        long currentSize = 0;
                        long totalSize = Model.Archive.Entries.Sum(_ => _.CompressedSize);

                        Model.SaveDataEntry(reader, writer, entry);
                        CurrentProgress = (int)(currentSize * 100.0 / totalSize);
                        CurrentFile = entry.Hash.ToString();
                        currentSize += entry.CompressedSize;
                    }

                    Model.UpdateSavedRpkgFileStructure(writer);
                }
            }

            OnPropertyChanged("Model");
        }

        public string GenerateRandomName()
        {
            Random generator = new Random();
            return Path.ChangeExtension(LoadedFilePath, @".tmp_" + generator.Next().ToString());
        }

        public int GetRpkgVersion(string filename)
        {
            return LoadedFilePath.Contains("patch") ? 1 : 0;
        }
    }
}
