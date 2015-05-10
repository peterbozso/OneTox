using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SharpTox.Core;
using WinTox.ViewModel;

namespace WinTox.View
{
    public sealed partial class ProfileSettingsFlyout : SettingsFlyout
    {
        private readonly ProfileSettingsViewModel _viewModel;

        public ProfileSettingsFlyout()
        {
            InitializeComponent();
            _viewModel = DataContext as ProfileSettingsViewModel;
            StatusComboBox.ItemsSource = Enum.GetValues(typeof (ToxUserStatus)).Cast<ToxUserStatus>();
        }

        private void NameTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == String.Empty)
                textBox.Text = _viewModel.Name;
        }

        private void CopyButtonClick(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(ToxIdTextBlock.Text);
            Clipboard.SetContent(dataPackage);
        }

        private void NospamButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.RandomizeNospam();
        }

        private async void ExportButtonClick(object sender, RoutedEventArgs e)
        {
            var file = await PickDestinationFile();
            if (file != null)
            {
                var successfulExport = await _viewModel.ExportProfile(file, PasswordTextBox.Text);
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

        private async void ProfileSettingsFlyoutLostFocus(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveDataAsync();
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

        private async void NewButtonClick(object sender, RoutedEventArgs e)
        {
            await _viewModel.CreateNewProfile();
        }

        private async void SwitchButtonClick(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshProfileList();
        }

        private async void ProfileNameListItemClick(object sender, ItemClickEventArgs e)
        {
            await _viewModel.SwitchProfile(e.ClickedItem as StorageFile);
        }
    }
}