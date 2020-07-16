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
        private int myCurrentProgress;
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
            string[] infoNames = new string[] { "EGLD", "RCOL", "VLTR", "FXFG" };
            Dictionary<string, Dictionary<UInt64, RpkgEntry>> fileMap = new Dictionary<string, Dictionary<ulong, RpkgEntry>>();

            foreach(string infoName in infoNames)
            {
                fileMap[infoName] = new Dictionary<UInt64, RpkgEntry>();
            }

            foreach(RpkgEntry entry in Model.Archive.Entries)
            {
                if (infoNames.Contains(entry.Info.Signature))
                {
                    fileMap[entry.Info.Signature][entry.Hash] = entry;
                }
            }

            string[] fileList = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            foreach (string file in fileList)
            {
                string sufix = Path.GetFileNameWithoutExtension(LoadedFilePath);
                string specificFile = file + @"_" + sufix;
                if (fileList.Contains(specificFile) || (file.Contains("_") && !file.EndsWith(sufix)))
                {
                    continue;
                }

                string[] tokens = file.Split(new string[] { directory + @"\" }, StringSplitOptions.RemoveEmptyEntries);
                if (!String.IsNullOrWhiteSpace(tokens[0]))
                {
                    string[] filepath = tokens[0].Split('\\');
                    if (fileMap.ContainsKey(filepath[0]))
                    {
                        UInt64 hash = Convert.ToUInt64(filepath[1].Split(new string[] { ".dat" }, StringSplitOptions.None)[0], 16);
                        if(fileMap[filepath[0]].ContainsKey(hash))
                        {
                            RpkgEntry currentEntry = fileMap[filepath[0]][hash];

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


        public void SaveStructureByRepack(string path)
        {
            using (RpkgBinaryReader reader = new RpkgBinaryReader(GetRpkgVersion(LoadedFilePath), File.Open(LoadedFilePath, FileMode.Open)))
            {
                using (RpkgBinaryWriter writer = new RpkgBinaryWriter(GetRpkgVersion(path), File.Open(path, FileMode.Create)))
                {
                    Model.SaveOriginalRpkgFileStructure(writer);

                    long currentSize = 0;
                    long totalSize = Model.Archive.Entries.Sum(_ => _.CompressedSize);

                    foreach (RpkgEntry entry in Model.Archive.Entries)
                    {
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

        public void SaveStructureByAppend()
        {
            using (RpkgBinaryWriter writer = new RpkgBinaryWriter(GetRpkgVersion(LoadedFilePath), File.Open(LoadedFilePath, FileMode.Open, FileAccess.ReadWrite)))
            {
                long currentSize = 0;
                var entries = Model.Archive.Entries.Where(_ => _.Import != null).ToList();
                long totalSize = entries.Sum(_ => _.CompressedSize);

                foreach (RpkgEntry entry in entries)
                {
                    Model.AppendDataEntry(writer, entry);
                    CurrentProgress = (int)(currentSize * 100.0 / totalSize);
                    CurrentFile = entry.Hash.ToString();
                    currentSize += entry.CompressedSize;
                }

                Model.UpdateSavedRpkgFileStructure(writer);
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
