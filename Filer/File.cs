using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace ReFiler
{
    public class File
    {
        public string path;
        public StorageFile file;
        public BitmapImage image { get; set; }

        public File(StorageFile file)
        {
            this.file = file;
        }
    }
}