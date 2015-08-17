using System.Collections.Specialized;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OneTox.View.UserControls.Friends;
using OneTox.View.UserControls.ProfileSettings;
using OneTox.ViewModel;
using OneTox.ViewModel.Friends;

namespace OneTox.View.Pages
{
    public sealed partial class MainPage : Page
    {
        private readonly PageStateInitializator _pageStateInitializator;
        private MainViewModel _mainViewModel;

        public MainPage()
        {
            InitializeComponent();

            _pageStateInitializator = new PageStateInitializator(this);

            ChangeLayoutBasedOnWindowWidth(Window.Current.Bounds.Width);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _pageStateInitializator.HandleOnNavigatedToParameter(e.Parameter);
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += WindowSizeChanged;

            DataContext = _mainViewModel = (Application.Current as App).MainViewModel;
            _mainViewModel.FriendList.Friends.CollectionChanged += FriendsCollectionChangedHandler;

            _pageStateInitializator.SelectFriendOnLoadedIfNeeded(FriendList, _mainViewModel);
        }

        private void MainPageUnloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= WindowSizeChanged;
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            ChangeLayoutBasedOnWindowWidth(e.Size.Width);
        }

        private void ChangeLayoutBasedOnWindowWidth(double width)
        {
            if (width < 930)
            {
                LeftPanel.Visibility = Visibility.Collapsed;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += MainPageBackRequestedHandler;

                VisualStateManager.GoToState(ChatBlock, "NarrowState", true);
                VisualStateManager.GoToState(ProfileSettingsBlock, "NarrowState", true);
            }
            else
            {
                LeftPanel.Visibility = Visibility.Visible;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().BackRequested -= MainPageBackRequestedHandler;

                VisualStateManager.GoToState(ChatBlock, "WideState", true);
                VisualStateManager.GoToState(ProfileSettingsBlock, "WideState", true);
            }
        }

        private void MainPageBackRequestedHandler(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof (FriendListPage));

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= MainPageBackRequestedHandler;
        }

        private void FriendListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FriendList.SelectedItem == null)
                return;

            VisualStateManager.GoToState(this, "ChatState", true);

            ChatBlock.DataContext = FriendList.SelectedItem as FriendViewModel;
        }

        private void AddFriendButtonClick(object sender, RoutedEventArgs e)
        {
            FriendList.SelectedItem = null;
            VisualStateManager.GoToState(this, "AddFriendState", true);
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            FriendList.SelectedItem = null;
            VisualStateManager.GoToState(this, "SettingsState", true);
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

        #region Page state initializator

        private class PageStateInitializator
        {
            private readonly MainPage _mainPage;
            private FriendViewModel _friendToSelectOnLoaded;
            private bool _selectFriendOnLoaded;

            public PageStateInitializator(MainPage mainPage)
            {
                _mainPage = mainPage;
            }

            public void HandleOnNavigatedToParameter(object parameter)
            {
                if (parameter == null)
                {
                    VisualStateManager.GoToState(_mainPage, "ChatState", false);
                    _selectFriendOnLoaded = true;
                    _friendToSelectOnLoaded = null;
                    return;
                }

                if (parameter is FriendViewModel)
                {
                    VisualStateManager.GoToState(_mainPage, "ChatState", false);
                    _selectFriendOnLoaded = true;
                    _friendToSelectOnLoaded = parameter as FriendViewModel;
                    return;
                }

                if (Equals(parameter, typeof (ProfileSettingsBlock)))
                {
                    VisualStateManager.GoToState(_mainPage, "SettingsState", false);
                    _selectFriendOnLoaded = false;
                    _friendToSelectOnLoaded = null;
                    return;
                }

                if (Equals(parameter, typeof (AddFriendBlock)))
                {
                    VisualStateManager.GoToState(_mainPage, "AddFriendState", false);
                    _selectFriendOnLoaded = false;
                    _friendToSelectOnLoaded = null;
                }
            }

            public void SelectFriendOnLoadedIfNeeded(ListView friendList, MainViewModel mainViewModel)
            {
                // TODO: Remember which friend we talked to the last time before shutting down the app and resume with selecting him/her.
                // TODO: Handle the case when the user doesn't have any friends yet with a splash screen or something like that!

                if (!_selectFriendOnLoaded)
                    return;

                friendList.SelectedItem = _friendToSelectOnLoaded;

                if (friendList.SelectedItem == null && mainViewModel.FriendList.Friends.Count > 0)
                {
                    friendList.SelectedItem = mainViewModel.FriendList.Friends[0];
                }
            }
        }

        #endregion
    }
}