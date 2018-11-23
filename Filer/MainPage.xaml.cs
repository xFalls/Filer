using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Filer;
using Filer.Annotations;
using IStorageItem = Windows.Storage.IStorageItem;


namespace MediFiler


{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly FilerModel model;
        private readonly FilerController controller;


        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.KeyDown += Page_KeyDown;

            model = new FilerModel();
            FilerView view = new FilerView(model, this);
            controller = new FilerController(model, view);
        }



        public void RefreshFolderList()
        {
            FolderPanel.Children.Clear();
            BuildFolderList(model.RootFolder, 0);
        }

        public void BuildFolderList(Folder folder, int depth)
        {
            depth++;
            Button folderButton = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(25*depth,4,8,5),
                Content = folder.ToString(),
                Tag = folder
            };
            folderButton.Click += FolderButton_Click;

            FolderPanel.Children.Add(folderButton);

            foreach (Folder folderChild in folder.FolderChildren)
            {
                BuildFolderList(folderChild, depth);
            }
        }


        //// Window events /////////////////////////////////////////////////////////

        // Load folder represented by clicked button
        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            Folder clicked = (Folder) ((Button) e.OriginalSource).Tag;
            controller.LoadNewFolder(clicked);
        }


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

        // Display popout when dragging files over the window
        private void OnFileDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            if (e.DragUIOverride != null)
            {
                e.DragUIOverride.Caption = "Add folder";
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
                        Folder rootFolder = new Folder((StorageFolder) items[0], null);
                        Task t = GetFolderStructure(folder, rootFolder);
                        await t;

                        model.RootFolder = rootFolder;
                    }
                    // Load the root folder on new file drop
                    controller.LoadNewFolder(model.RootFolder);
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
                    Folder foundFolder = new Folder((StorageFolder) item, parent);
                    parent.AddFolder(foundFolder);

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
        private void Grid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            int mouseDelta = e.GetCurrentPoint(ContentGrid).Properties.MouseWheelDelta;
            if (mouseDelta > 0)
                controller.Move(-1);
            else
                controller.Move(1);
        }


        // Show folderlist on mouse hover
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FolderList.Visibility = Visibility.Visible;
        }

        // Hide folderlist on mouse leave
        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FolderList.Visibility = Visibility.Collapsed;
        }
    }
}
