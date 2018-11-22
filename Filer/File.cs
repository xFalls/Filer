using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;

namespace Filer
{
    public class File
    {
        private string path;
        public BitmapImage image { get; set; }

        public File(string path)
        {
            this.path = path;
            Debug.Print(path + " added\n");
        }
    }
}