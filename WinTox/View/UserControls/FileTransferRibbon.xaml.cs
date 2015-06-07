using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTox.ViewModel.FileTransfer;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WinTox.View.UserControls
{
    public sealed partial class FileTransferRibbon : UserControl
    {
        public FileTransferRibbon()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var transferViewModel = (OneFileTransferViewModel) DataContext;
            VisualStateManager.GoToState(this, transferViewModel.State.ToString(), true);
        }

        private async void AcceptButtonClick(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            var saveFolder = await folderPicker.PickSingleFolderAsync();
            if (saveFolder == null)
                return;

            var transferViewModel = (OneFileTransferViewModel) DataContext;
            var saveFile =
                await saveFolder.CreateFileAsync(transferViewModel.Name, CreationCollisionOption.GenerateUniqueName);
            await transferViewModel.AcceptTransferByUser(saveFile);
        }
    }
}