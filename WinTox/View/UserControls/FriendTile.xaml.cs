using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace WinTox.View.UserControls
{
    public sealed partial class FriendTile : UserControl
    {
        public FriendTile()
        {
            InitializeComponent();
        }

        private void MainGridRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement) sender);
            e.Handled = true;
        }

        private void MainGridTapped(object sender, TappedRoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof (ChatPage), DataContext);
        }
    }
}