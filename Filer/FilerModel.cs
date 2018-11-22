using System;
using System.Collections.Generic;
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
        public List<Folder> ListOfFolders = new List<Folder>();
        public List<File> LoadedFiles = new List<File>();

        private int FileIndex;

        public FilerModel()
        {

        }

        public BitmapImage GetContent()
        {
            return LoadedFiles[FileIndex].image;
        }

        /*public WriteableBitmap ConvertWebPtoBitmap()
        {

        }*/
    }
}