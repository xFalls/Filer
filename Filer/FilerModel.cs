using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using ReFiler;

namespace ReFiler
{
    public class FilerModel
    {
        //public List<Folder> ListOfFolders { get; } = new List<Folder>();
        public Folder RootFolder { get; set; }
        public List<File> LoadedFiles = new List<File>();
        public Folder ActiveFolder;

        public int FileIndex { get; private set; }
        public bool Loaded { get; private set; }
        public bool KeyBlockingMode;


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

        public void UnloadAll()
        {
            ClearContext();
            RootFolder = null;
            Loaded = false;
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




        // Goes through a folder and its subfolders adding each directory and file to a list
        public async Task GetFolderStructure(IStorageFolder folder, Folder parent)
        {
            IReadOnlyList<IStorageItem> items = await folder.GetItemsAsync();

            foreach (IStorageItem item in items)
            {
                if (item is StorageFolder)
                {
                    // Add folder, and scan it as well
                    Folder foundFolder = new Folder((StorageFolder)item, parent);
                    parent.AddFolder(foundFolder);

                    Task t = GetFolderStructure((StorageFolder)item, foundFolder);
                    await t;
                }
                else
                {
                    // Add found files
                    parent.AddFiles(new File((StorageFile)item));
                }
            }
        }
    }
}