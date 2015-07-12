using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace WinTox.View.UserControls.FriendRequests
{
    public sealed partial class FriendRequestsListItem : UserControl
    {
        public FriendRequestsListItem()
        {
            InitializeComponent();
        }

        private void ContentGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement) sender);
        }
    }
}