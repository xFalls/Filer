using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MediFiler
{


    public class Content
    {
        public StorageFile content;

        public Content(StorageFile file)
        {
            content = file;
        }
    }

    public class ImageContent : Content
    {
        public ImageContent(StorageFile file) : base(file)
        {
        }
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private FilerModel model;
        FilerController controller;

        public MainPage()
        {
            this.InitializeComponent();

            model = new FilerModel();
            FilerView view = new FilerView(model, this);
            controller = new FilerController(model, view);
        }
    

        //// Window events /////////////////////////////////////////////////////////


        // Adjusts image resolution according to window size
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (imgMainContent.Source != null)
            {
                if (imgMainContent.Source is BitmapImage image)
                {
                    image.DecodePixelHeight = (int)ContentGrid.ActualHeight;
                }
                else if (imgMainContent.Source is WriteableBitmap)
                {
                    //
                }
            }
        }


        private void OnFileDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            if (e.DragUIOverride != null)
            {
                e.DragUIOverride.Caption = "Add files";
                e.DragUIOverride.IsContentVisible = true;
            }
        }


        private void OnFileDragLeave(object sender, DragEventArgs e)
        {

        }

        private async void OnFileDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    model.ContentList.Add(new ImageContent(items[0] as StorageFile));
                    controller.RefreshView();
                }
            }
        }

        private void Page_PreviewKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("h");
            switch (e.Key)
            {
                case VirtualKey.Right:
                    //ContentPosition++;
                    //RefreshView();
                    break;

                case VirtualKey.Left:
                    //ContentPosition--;
                    //RefreshView();
                    break;
            }
        }
    }
}
