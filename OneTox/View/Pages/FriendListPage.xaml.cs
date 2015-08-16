using System.Collections.Specialized;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OneTox.View.UserControls.Messaging;
using OneTox.View.UserControls.ProfileSettings;
using OneTox.ViewModel;

namespace OneTox.View.Pages
{
    public sealed partial class FriendListPage : Page
    {
        private MainViewModel _mainViewModel;

        public FriendListPage()
        {
            InitializeComponent();

            ChangeLayoutBasedOnWindowWidth(Window.Current.Bounds.Width);
        }

        private void FriendListPageLoaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += WindowSizeChanged;
            DataContext = _mainViewModel = (App.Current as App).MainViewModel;
            _mainViewModel.FriendList.Friends.CollectionChanged += FriendsCollectionChangedHandler;
        }

        private void FriendListPageUnloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= WindowSizeChanged;
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            ChangeLayoutBasedOnWindowWidth(e.Size.Width);
        }

        private void ChangeLayoutBasedOnWindowWidth(double width)
        {
            if (width >= 930)
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void FriendListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FriendList.SelectedItem == null)
                return;
            /*
            VisualStateManager.GoToState(this, "ChatState", true);

            ChatBlock.SetDataContext(FriendList.SelectedItem as FriendViewModel);
            */
        }

        private void AddFriendButtonClick(object sender, RoutedEventArgs e)
        {
            /*
            FriendList.SelectedItem = null;
            VisualStateManager.GoToState(this, "AddFriendState", true);
            */
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            /*
            FriendList.SelectedItem = null;
            VisualStateManager.GoToState(this, "SettingsState", true);
            */
        }

        private void FriendsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldStartingIndex == -1)
                return;

            if (FriendList.SelectedItem == null) // It means that we just removed the currently selected friend.
            {
                // So select the one right above it:
                FriendList.SelectedItem = (e.OldStartingIndex - 1) > 0
                    ? _mainViewModel.FriendList.Friends[e.OldStartingIndex - 1]
                    : _mainViewModel.FriendList.Friends[0];
            }
        }
    }
}