using OneTox.ViewModel.ProfileSettings;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private async void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            await _viewModel.DeleteProfile(ProfileList.SelectedItem as ProfileViewModel);
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

        private async void ProfileManagementBlockLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshProfileList();
        }

        private async void SwitchButtonClick(object sender, RoutedEventArgs e)
        {
            await (ProfileList.SelectedItem as ProfileViewModel).SetAsCurrent();
            await _viewModel.RefreshProfileList();
        }
    }
}
