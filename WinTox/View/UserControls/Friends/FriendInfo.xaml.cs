using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinTox.View.UserControls.Friends
{
    public sealed partial class FriendInfo : UserControl
    {
        public FriendInfo()
        {
            InitializeComponent();
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof (MainPage));
        }
    }
}