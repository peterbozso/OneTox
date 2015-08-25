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

        private async void ProfileManagementBlockLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshProfileList();
        }
    }
}
