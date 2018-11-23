using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Filer.Annotations;

namespace Filer
{
    public class Folder
    {
        public ObservableCollection<Folder> FolderChildren { get; set; } = new ObservableCollection<Folder>();
        public List<File> ListOfFiles = new List<File>();

        public StorageFolder folder;
        public string Name;

        public Folder(StorageFolder folder, Folder parentFolder)
        {
            this.folder = folder;
            Name = folder.Name;
        }


        


        public void AddFolder(Folder newFolders)
        {
            FolderChildren.Add(newFolders);
        }

        public void AddFiles(File files)
        {
            ListOfFiles.Add(files);
        }

        public override string ToString()
        {
            return "[" + ListOfFiles.Count + "] " + Name;
        }
    }
}