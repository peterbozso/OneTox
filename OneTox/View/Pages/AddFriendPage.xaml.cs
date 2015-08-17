using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneTox.View.Pages
{
    public sealed partial class AddFriendPage : Page
    {
        public AddFriendPage()
        {
            InitializeComponent();
        }

        private void AddFriendPageLoaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += AddFriendPageBackRequested;

            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void AddFriendPageUnloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= AddFriendPageBackRequested;

            Window.Current.SizeChanged -= WindowSizeChanged;
        }

        private void AddFriendPageBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof (FriendListPage));
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 930)
            {
                Frame.Navigate(typeof (MainPage), typeof (AddFriendPage));
            }
        }
    }
}