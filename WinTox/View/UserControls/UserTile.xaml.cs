using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace WinTox.View.UserControls
{
    public sealed partial class UserTile : UserControl
    {
        public UserTile()
        {
            InitializeComponent();
            DataContext = App.UserViewModel;
        }

        private void UserTileTapped(object sender, TappedRoutedEventArgs e)
        {
            App.ShowProfileSettingsFlyout();
        }
    }
}