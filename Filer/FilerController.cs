using System;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace MediFiler
{
    public class FilerController
    {
        private readonly FilerModel model;
        private readonly FilerView view;

        public FilerController(FilerModel model, FilerView view)
        {
            this.model = model;
            this.view = view;
        }

        public void Move(int cursor)
        {
            model.ViewRelative(cursor);
            RefreshView();
        }

        public void InitializeView()
        {
            // Loads root files into active context TODO temp
            model.LoadContext();

            view.window.RefreshFolderList();
        }


        public async void RefreshView()
        {
            // Don't do anything if nothing is loaded
            if (!model.Loaded || model.RootFolder.ListOfFiles.Count == 0)
            {
                return;
            }
            


            StorageFile storageFile = model?.RootFolder.ListOfFiles[model.FileIndex].file;

            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
            bitmapImage.DecodePixelHeight = (int)view.window.ContentGrid.ActualHeight;

            // Set the image on the main page to the dropped image
            view.window.imgMainContent.Source = bitmapImage;
        }
    }
}