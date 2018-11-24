using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using ReFiler;
using ReFiler.Annotations;
using IStorageItem = Windows.Storage.IStorageItem;


namespace ReFiler


{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly FilerModel model;
        private readonly FilerController controller;

        private SolidColorBrush setBrush = new SolidColorBrush(Colors.DeepSkyBlue);
        private SolidColorBrush artistBrush = new SolidColorBrush(Colors.GreenYellow);


        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.KeyDown += Page_KeyDown;

            model = new FilerModel();
            FilerView view = new FilerView(model, this);
            controller = new FilerController(model, view);

            SetTitle("ReFiler");

            model.UnloadAll();
            MostRecentlyOpened();
        }

        public void SetTitle(string text)
        {
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = text;
        }

        public void SetInfobar(string text)
        {
            Infobar.Text = text;
        }


        public void RefreshFolderList()
        {
            FolderPanel.Children.Clear();
            BuildFolderList(model.RootFolder, 0);
        }

        public void BuildFolderList(Folder folder, int depth)
        {
            depth++;

            string text = folder.Name;

            Button folderButton = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(25*depth,4,8,5),
                Content = folder.ToString(),
                Tag = folder
            };
            folderButton.Click += FolderButton_Click;
            folderButton.RightTapped += FolderButton_RightClick;

            // Color folders based on content
            if (text.StartsWith("[Set]"))
            {
                folderButton.Foreground = setBrush;
            } else if (text.StartsWith("[Artist]"))
            {
                folderButton.Foreground = artistBrush;
            }

            FolderPanel.Children.Add(folderButton);

            foreach (Folder folderChild in folder.FolderChildren)
            {
                BuildFolderList(folderChild, depth);
            }
        }

        public async void MostRecentlyOpened()
        {
            StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;
            RecentFoldersPanel.Children.Clear();

            foreach (AccessListEntry entry in mru.Entries)
            {
                string mruToken = entry.Token;
                string mruMetadata = entry.Metadata;
                IStorageFolder item = await mru.GetFolderAsync(mruToken);
                // The type of item will tell you whether it's a file or a folder.

                Button folderButton = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Content = item.Name,
                    Tag = item
                };
                folderButton.Click += RecentFolderButton_Click;
                RecentFoldersPanel.Children.Add(folderButton);
            }
        }

        public void HideMainMenu()
        {
            AddFolderFromHome.Visibility = Visibility.Collapsed;
            RecentFoldersPanel.Visibility = Visibility.Collapsed;
        }

        public void ShowMainMenu()
        {
            AddFolderFromHome.Visibility = Visibility.Visible;
            RecentFoldersPanel.Visibility = Visibility.Visible;
        }


        //// Window events /////////////////////////////////////////////////////////

        // Load folder represented by clicked button
        private async void RecentFolderButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = (StorageFolder)((Button)e.OriginalSource).Tag;

            if (folder == null) return;
            Folder rootFolder = new Folder(folder, null);
            Task t = model.GetFolderStructure(folder, rootFolder);
            await t;

            model.RootFolder = rootFolder;
            controller.LoadNewFolder(model.RootFolder);

            var mru = StorageApplicationPermissions.MostRecentlyUsedList;
            mru.Add(folder, folder.Path);

            if (model.Loaded)
            {
                HideMainMenu();
            }
        }


        // Load folder represented by clicked button
        private async void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            Folder clicked = (Folder) ((Button) e.OriginalSource).Tag;


            clicked.ListOfFiles.Clear();
            IReadOnlyList<IStorageItem> items = await clicked.folder.GetItemsAsync();

            foreach (IStorageItem item in items)
            {
                if (item is StorageFile)
                {
                    // Add found files
                    clicked.AddFiles(new File((StorageFile)item));
                }
            }

            controller.LoadNewFolder(clicked);

            
        }

        // Load folder represented by clicked button
        private async void FolderButton_RightClick(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                Folder clicked = (Folder)((Button)sender).Tag;
                File fileToMove = model.ActiveFolder.ListOfFiles[model.FileIndex];

                if (model.ActiveFolder.ListOfFiles.Count == 0) return;

                await fileToMove.file.MoveAsync(clicked.folder, fileToMove.file.Name, 
                    NameCollisionOption.GenerateUniqueName);

                clicked.ListOfFiles.Add(fileToMove);
                //clicked.ListOfFiles.Sort();
                model.ActiveFolder.ListOfFiles.RemoveAt(model.FileIndex);
                model.LoadedFiles.RemoveAt(model.FileIndex);
                

                if (model.FileIndex >= model.LoadedFiles.Count)
                {
                    controller.Move(-1);
                }

                controller.RefreshView();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                controller.RefreshView();
            }
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
                        Task t = model.GetFolderStructure(folder, rootFolder);
                        await t;

                        model.RootFolder = rootFolder;
                    }
                    // Load the root folder on new file drop
                    controller.LoadNewFolder(model.RootFolder);
                }
            }
        }


        // Handles keyboard inputs
        private void Page_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            if (model.KeyBlockingMode) return;

            switch (e.VirtualKey)
            {
                // Next file
                case VirtualKey.Right:
                    controller.Move(1);
                    break;

                // Previous file
                case VirtualKey.Left:
                    controller.Move(-1);
                    break;

                // TODO: Open manual
                case VirtualKey.F1:
                    
                    break;

                // Rename
                case VirtualKey.F2:
                    controller.RenameFile();
                    break;

                // Append +
                case VirtualKey.F3:
                    controller.PrefixAdd();
                    break;

                // Remove +
                case VirtualKey.F4:
                    controller.PrefixRemove();
                    break;

                // Refresh
                case VirtualKey.F5:
                    controller.Refresh();
                    break;

                // Return to home
                case VirtualKey.F6:
                    controller.LoadMainMenu();
                    break;

                // TODO: Fullscreen
                case VirtualKey.F11:
                    
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

        // Add a new folder
        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            controller.NewFolder();
        }

        // Right click content to open it
        private async void ImgMainContent_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            try
            {
                StorageFile file = model.LoadedFiles[model.FileIndex].file;
                await Launcher.LaunchFileAsync(file);
            }
            catch (Exception error)
            {
                Console.WriteLine("Failed launching file: " + error);
            }
        }
    }
}
