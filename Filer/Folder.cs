using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage;

namespace Filer
{
    public class Folder
    {
        private List<Folder> ListOfFolders = new List<Folder>();
        private List<File> ListOfFiles = new List<File>();

        private string path;

        public Folder(string path, Folder parentFolder)
        {
            this.path = path;
            Debug.Print(path + " added\n");
        }

        public void AddFolders(Folder folders)
        {
            ListOfFolders.Add(folders);
        }

        public void AddFiles(File files)
        {
            ListOfFiles.Add(files);
        }

    }
}