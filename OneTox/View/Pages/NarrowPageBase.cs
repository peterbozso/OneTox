using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneTox.View.Pages
{
    public abstract class NarrowPageBase : Page
    {
        protected NarrowPageBase()
        {
            Loaded += PageLoaded;
            Unloaded += PageUnloaded;
        }

        protected abstract void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e);

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof (FriendListPage));
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequested;

            Window.Current.SizeChanged -= WindowSizeChanged;
        }
    }
}