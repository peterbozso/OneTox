using System;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OneTox.ViewModel.ProfileSettings;

namespace OneTox.View.UserControls
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

            // Show the settings again when we return, in case the user want to do more than just exporting once.
            App.ShowProfileSettingsFlyout();
        }

        private async void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.ImportProfile();

                // Show the settings again when we return, in case the user want to do more than just exporting once.
                App.ShowProfileSettingsFlyout();
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