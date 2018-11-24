using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        public async Task FolderPicker()
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();

            if (folder == null) return;
            Folder rootFolder = new Folder(folder, null);
            Task t = model.GetFolderStructure(folder, rootFolder);
            await t;

            model.RootFolder = rootFolder;
            LoadNewFolder(model.RootFolder);
        }

        public async void NewFolder()
        {
            await FolderPicker();

            if (model.Loaded)
            {
                view.window.HideMainMenu();
            }
        }

        public void LoadMainMenu()
        {
            if (model.Loaded)
            {
                model.UnloadAll();
                RefreshView();
                view.window.ShowMainMenu();
                view.window.SetTitle("ReFiler");
            }
        }

        // Rename dialog
        private async Task<string> InputTextDialogAsync(string title, string defaultText)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            inputTextBox.Text = defaultText;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Rename";
            dialog.SecondaryButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }


        public async void RenameFile()
        {
            try
            {
                StorageFile currentFile = model.LoadedFiles[model.FileIndex].file;

                model.KeyBlockingMode = true;
                string newName = await InputTextDialogAsync("Input new name", currentFile.DisplayName);
                model.KeyBlockingMode = false;

                if (newName == "") return;

                await currentFile.RenameAsync(newName + currentFile.FileType, NameCollisionOption.GenerateUniqueName);
                RefreshView();
            }
            catch (InvalidOperationException e)
            {
                model.KeyBlockingMode = false;
                Console.WriteLine("Other operation pending");
            }
        }

        public async void PrefixAdd()
        {
            try
            {
                StorageFile currentFile = model.LoadedFiles[model.FileIndex].file;
                await currentFile.RenameAsync("+" + currentFile.Name, NameCollisionOption.GenerateUniqueName);
                RefreshView();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Other operation pending");
            }
        }

        public async void PrefixRemove()
        {
            try
            {
                StorageFile currentFile = model.LoadedFiles[model.FileIndex].file;
                if (currentFile.Name.StartsWith("+"))
                {
                    await currentFile.RenameAsync(currentFile.Name.Substring(1), NameCollisionOption.GenerateUniqueName);
                }
                RefreshView();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Other operation pending");
            }
        }


        public async void RefreshView()
        {
            try
            {
                // Show nothing if fileindex is outside what should be possible
                if (model.FileIndex < 0 || model.FileIndex > model.LoadedFiles.Count)
                {
                    view.window.imgMainContent = null;
                    return;
                }

                view.window.RefreshFolderList();
                StorageFile storageFile = model.LoadedFiles[model.FileIndex].file;

                view.window.SetTitle("ReFiler - (" + 
                                     (model.FileIndex + 1) + "/" + model.LoadedFiles.Count + ") - "
                                     + storageFile.Name);

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
                view.window.imgMainContent.Source = null;
                Debug.WriteLine("Error loading file\n" + e);
            }
            
        }
    }
}