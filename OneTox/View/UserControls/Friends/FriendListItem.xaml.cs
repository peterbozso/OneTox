using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace OneTox.View.UserControls.Friends
{
    public sealed partial class FriendListItem : UserControl
    {
        public FriendListItem()
        {
            InitializeComponent();
        }

        private void MainGridRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement) sender);
            e.Handled = true;
        }
    }
}