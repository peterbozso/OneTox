using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OneTox.ViewModel.ProfileSettings;

namespace OneTox.View.ProfileSettings.Controls
{
    public sealed partial class ProfileManagementBlock : UserControl
    {
        public ProfileManagementBlock()
        {
            InitializeComponent();
        }

        private async void ProfileManagementBlockLoaded(object sender, RoutedEventArgs e)
        {
            await (DataContext as ProfileManagementViewModel).RefreshProfileList();
        }
    }
}