using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneTox.View.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();

            VisualStateManager.GoToState(ProfileSettingsBlock, "NarrowState", false);
        }

        private void SettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += SettingsPageBackRequested;

            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void SettingsPageUnloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= SettingsPageBackRequested;

            Window.Current.SizeChanged -= WindowSizeChanged;
        }

        private void SettingsPageBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof (FriendListPage));
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 930)
            {
                Frame.Navigate(typeof (MainPage), typeof (SettingsPage));
            }
        }
    }
}