using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
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
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Tox save file", new List<string>() { ".tox" });
            savePicker.SuggestedFileName = _viewModel.Name;
            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var successfulExport = await _viewModel.ExportProfile(file);
                if (!successfulExport)
                {
                    var msgDialog = new MessageDialog("Unsuccesfull export: the file couldn't be saved.");
                    msgDialog.ShowAsync();
                }
            }
        }

        private async void ProfileSettingsFlyoutLostFocus(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveDataAsync();
        }
    }
}