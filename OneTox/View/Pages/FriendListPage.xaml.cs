using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OneTox.ViewModel;
using OneTox.ViewModel.Friends;

namespace OneTox.View.Pages
{
    public sealed partial class FriendListPage : Page
    {
        public FriendListPage()
        {
            InitializeComponent();
        }

        private void AddFriendButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (AddFriendPage));
        }

        private void FriendListPageLoaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void FriendListPageUnloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= WindowSizeChanged;
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (SettingsPage));
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 930)
            {
                Frame.Navigate(typeof (MainPage));
            }
        }

        private void FriendListItemClick(object sender, ItemClickEventArgs e)
        {
            var friendListViewModel = DataContext as FriendListViewModel;
            friendListViewModel.SelectedFriend = e.ClickedItem as FriendViewModel;

            Frame.Navigate(typeof (ChatPage));
        }
    }
}