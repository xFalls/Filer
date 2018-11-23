using System.Collections.Generic;
using System.Linq;
using ReFiler;

namespace ReFiler
{
    public class FilerModel
    {
        //public List<Folder> ListOfFolders { get; } = new List<Folder>();
        public Folder RootFolder { get; set; }
        private List<File> LoadedFiles = new List<File>();
        public Folder ActiveFolder;

        public int FileIndex { get; private set; }
        public bool Loaded { get; private set; }


        public void LoadContext(Folder folder)
        {
            ClearContext();

            ActiveFolder = folder;
            LoadedFiles.AddRange(folder.ListOfFiles);
            Loaded = true;
        }


        public void ClearContext()
        {
            FileIndex = 0;
            LoadedFiles.Clear();
            ActiveFolder = null;
        }

        public void ViewRelative(int cursor)
        {
            // Avoid going outside list
            if (FileIndex + cursor >= 0 && FileIndex + cursor < LoadedFiles.Count)
            {
                FileIndex += cursor;
            }
        }

        public static bool IsImageExtension(string source)
        {
            return (source.EndsWith(".png") || source.EndsWith(".jpg") || source.EndsWith(".jpeg") ||
                    source.EndsWith(".webp") || source.EndsWith(".gif") || source.EndsWith(".bmp"));
        }
    }
}