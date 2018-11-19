using System;
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


        public async void RefreshView()
        {
            StorageFile storageFile = model.ContentList[model.ContentPosition].content;

            if (storageFile.FileType == ".webp")
            {

                byte[] bytes;

                using (var stream3 = await storageFile.OpenReadAsync())
                {
                    bytes = new byte[stream3.Size];
                    using (var reader = new DataReader(stream3))
                    {
                        await reader.LoadAsync((uint)stream3.Size);
                        reader.ReadBytes(bytes);
                    }
                }


                // Create an instance of the decoder
                var webp = new WebPDecoder();

                // Decode to BGRA (Bitmaps use this format)
                var pixelData = (await webp.DecodeBgraAsync(bytes)).ToArray();

                // Get the size
                var size = await webp.GetSizeAsync(bytes);

                // With the size of the webp, create a WriteableBitmap
				var bitmap = new WriteableBitmap((int)size.Width, (int)size.Height);

                // Write the pixel data to the buffer
                var stream = bitmap.PixelBuffer.AsStream();
                await stream.WriteAsync(pixelData, 0, pixelData.Length);

                // Set the bitmap
                view.window.imgMainContent.Source = bitmap;

                return;
            }

            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
            bitmapImage.DecodePixelHeight = (int)view.window.ContentGrid.ActualHeight;
            // Set the image on the main page to the dropped image
            view.window.imgMainContent.Source = bitmapImage;
        }
    }
}