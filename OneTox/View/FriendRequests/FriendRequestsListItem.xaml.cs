using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace OneTox.View.FriendRequests
{
    public sealed partial class FriendRequestsListItem : UserControl
    {
        public FriendRequestsListItem()
        {
            InitializeComponent();
        }

        private void PublicKeyTapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement) sender);
        }
    }
}