using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OneTox.ViewModel.Friends;

namespace OneTox.View.Pages
{
    public sealed partial class ChatPage : Page
    {
        private FriendViewModel _friendViewModel;

        public ChatPage()
        {
            InitializeComponent();

            VisualStateManager.GoToState(ChatBlock, "NarrowState", false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DataContext = _friendViewModel = e.Parameter as FriendViewModel;
        }

        private void ChatPageLoaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += ChatPageBackRequested;

            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void ChatPageUnloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= ChatPageBackRequested;

            Window.Current.SizeChanged -= WindowSizeChanged;
        }

        private void ChatPageBackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof (FriendListPage));
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 930)
            {
                Frame.Navigate(typeof (MainPage), _friendViewModel);
            }
        }
    }
}