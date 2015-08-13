using System;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OneTox.ViewModel.ProfileSettings;

namespace OneTox.View.UserControls.ProfileSettings
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
            await _viewModel.ExportProfile(PasswordTextBox.Text);
        }

        private async void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.ImportProfile();
            }
            catch
            {
                var msgDialog = new MessageDialog("Importing profile failed because of corrupted .tox file.",
                    "Error occurred");
                await msgDialog.ShowAsync();
            }
        }

        private async void ProfileNameListItemClick(object sender, ItemClickEventArgs e)
        {
            await _viewModel.SwitchProfile(e.ClickedItem as StorageFile);
        }
    }
}