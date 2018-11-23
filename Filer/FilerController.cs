using System;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using ReFiler;

namespace ReFiler
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

        // Remove old content and show new
        public void LoadNewFolder(Folder folder)
        {
            view.window.imgMainContent.Source = null;
            model.LoadContext(folder);
            view.window.RefreshFolderList();
            RefreshView();
        }


        public async void RefreshView()
        {
            try
            {
                StorageFile storageFile = model.ActiveFolder.ListOfFiles[model.FileIndex].file;

                if (FilerModel.IsImageExtension(storageFile.Name))
                {
                    // Image
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
                    bitmapImage.DecodePixelHeight = (int) view.window.ContentGrid.ActualHeight;

                    // Set the image on the main page to the dropped image
                    view.window.imgMainContent.Source = bitmapImage;
                }
                else
                {
                    // Thumbnail

                    StorageItemThumbnail thumb =
                        await storageFile.GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem);
                    if (thumb != null)
                    {
                        BitmapImage img = new BitmapImage();
                        await img.SetSourceAsync(thumb);
                        view.window.imgMainContent.Source = img;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading file\n" + e);
            }
            
        }
    }
}