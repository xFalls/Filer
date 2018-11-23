using System.Collections.Generic;
using Filer;

namespace MediFiler
{
    public class FilerModel
    {
        //public List<Folder> ListOfFolders { get; } = new List<Folder>();
        public Folder RootFolder { get; set; }
        private List<File> LoadedFiles = new List<File>();
        public Folder ActiveFolder;

        public int FileIndex { get; private set; }
        public bool Loaded { get; private set; }


        public void LoadContext()
        {
            LoadedFiles.AddRange(RootFolder.ListOfFiles);
            Loaded = true;
        }


        public void ClearContext()
        {
            FileIndex = 0;
            RootFolder = null;
            LoadedFiles.Clear();
        }

        public void ViewRelative(int cursor)
        {
            // Avoid going outside list
            if (FileIndex + cursor >= 0 && FileIndex + cursor < LoadedFiles.Count)
            {
                FileIndex += cursor;
            }
        }

        /*public WriteableBitmap ConvertWebPtoBitmap()
        {

        }*/
    }
}