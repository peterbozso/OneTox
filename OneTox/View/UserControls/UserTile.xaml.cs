using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace OneTox.View.UserControls
{
    public sealed partial class UserTile : UserControl
    {
        public UserTile()
        {
            InitializeComponent();
        }

        private void UserTileTapped(object sender, TappedRoutedEventArgs e)
        {
            App.ShowProfileSettingsFlyout();
        }
    }
}