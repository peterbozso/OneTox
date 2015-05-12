using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTox.ViewModel.ProfileSettings;

namespace WinTox.View.UserControls
{
    public sealed partial class ProfileManagementBlock : UserControl
    {
        private readonly ProfileManagementViewModel _viewModel;

        public ProfileManagementBlock()
        {
            InitializeComponent();
            _viewModel = DataContext as ProfileManagementViewModel;
        }

        private async void ExportButtonClick(object sender, RoutedEventArgs e)
        {
            var file = await PickDestinationFile();
            if (file != null)
            {
                var successfulExport =
                    await _viewModel.ExportProfile(file, PasswordTextBox.Text);
                if (!successfulExport)
                {
                    var msgDialog = new MessageDialog("Unsuccesfull export: the file couldn't be saved.");
                    msgDialog.ShowAsync();
                }
            }

            // Show the settings again when we return, in case the user want to do more than just exporting once.
            App.ShowProfileSettingsFlyout();
        }

        private async Task<StorageFile> PickDestinationFile()
        {
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Tox save file", new List<string> {".tox"});
            savePicker.SuggestedFileName = _viewModel.Name;
            var file = await savePicker.PickSaveFileAsync();
            return file;
        }

        private async void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            var file = await PickSourceFile();
            if (file != null)
            {
                await _viewModel.SetCurrentProfile(file);
            }

            // Show the settings again when we return, in case the user want to do more than just exporting once.
            App.ShowProfileSettingsFlyout();
        }

        private async Task<StorageFile> PickSourceFile()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".tox");
            return await openPicker.PickSingleFileAsync();
        }

        private async void ProfileNameListItemClick(object sender, ItemClickEventArgs e)
        {
            await _viewModel.SwitchProfile(e.ClickedItem as StorageFile);
        }
    }
}