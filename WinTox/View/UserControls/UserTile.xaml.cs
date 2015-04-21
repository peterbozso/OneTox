using Windows.UI.Xaml.Controls;

namespace WinTox.View.UserControls
{
    public sealed partial class UserTile : UserControl
    {
        public UserTile()
        {
            InitializeComponent();
            DataContext = App.UserViewModel;
        }

        private void UserTileTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            App.ShowProfileSettingsFlyout();
        }
    }
}