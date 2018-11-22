using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Universal.WebP;

namespace MediFiler
{
    public class FilerController
    {
        private FilerModel model;
        private FilerView view;

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
        }


        public async void RefreshView()
        {
            // Don't do anything if nothing is loaded
            if (!model.Loaded)
            {
                return;
            }

            StorageFile storageFile = model.ListOfFolders[0].ListOfFiles[model.FileIndex].file;

            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
            bitmapImage.DecodePixelHeight = (int)view.window.ContentGrid.ActualHeight;

            // Set the image on the main page to the dropped image
            view.window.imgMainContent.Source = bitmapImage;
        }
    }
}