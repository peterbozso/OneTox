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
            this.InitializeComponent();
        }

        private void MainGridRightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            e.Handled = true;
        }

        private void MainGridTapped(object sender, TappedRoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(ChatPage), DataContext);
        }
    }
}
