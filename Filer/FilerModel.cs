using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Filer;
using Universal.WebP;

namespace MediFiler
{
    public class FilerModel
    {
        public List<Folder> ListOfFolders { get; } = new List<Folder>();
        private List<File> LoadedFiles = new List<File>();

        public int FileIndex { get; private set; }
        public bool Loaded { get; private set; }

    public FilerModel()
        {

        }

        public void LoadContext()
        {
            LoadFilesFromRoot();
            Loaded = true;
        }

        public void LoadFilesFromRoot()
        {
            // TODO
            LoadedFiles.AddRange(ListOfFolders[0].ListOfFiles);
        }

        public void ClearContext()
        {
            FileIndex = 0;
            ListOfFolders.Clear();
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