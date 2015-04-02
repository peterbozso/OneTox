using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

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
    }
}
