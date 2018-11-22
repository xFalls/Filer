using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Filer;
using IStorageItem = Windows.Storage.IStorageItem;


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
        private FilerController controller;

        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.KeyDown += Page_KeyDown;

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
                IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    model.ClearContext();

                    if (items[0] is StorageFolder folder)
                    {
                        Folder rootFolder = new Folder(items[0].Path, null);
                        Task t = GetFolderStructure(folder, rootFolder);
                        await t;

                        // TODO
                        model.ListOfFolders.Add(rootFolder);
                    }
                    controller.InitializeView();
                    controller.RefreshView();
                }
            }
        }

        // Goes through a folder and its subfolders adding each directory and file to a list
        private async Task GetFolderStructure(IStorageFolder folder, Folder parent)
        {
            IReadOnlyList<IStorageItem> items = await folder.GetItemsAsync();

            foreach (IStorageItem item in items)
            {
                if (item is StorageFolder)
                {
                    // Add folder, and scan it as well
                    Folder foundFolder = new Folder(item.Path, parent);
                    parent.AddFolders(foundFolder);
                    model.ListOfFolders.Add(foundFolder);

                    Task t = GetFolderStructure((StorageFolder) item, foundFolder);
                    await t;
                }
                else
                {
                    // Add found files
                    parent.AddFiles(new File((StorageFile) item));
                }
            }
        }


        // Handles keyboard inputs
        private void Page_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            switch (e.VirtualKey)
            {
                case VirtualKey.Right:
                    controller.Move(1);
                    break;
                case VirtualKey.Left:
                    controller.Move(-1);
                    break;
            }
        }

        // Allows for navigation through scrolling
        private void ImgMainContent_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            int mouseDelta = e.GetCurrentPoint(ContentGrid).Properties.MouseWheelDelta;
            if (mouseDelta > 0)
                controller.Move(-1);
            else
                controller.Move(1);
        }
    }
}
