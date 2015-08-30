using System.Collections.Specialized;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OneTox.View.UserControls.Friends;
using OneTox.View.UserControls.Messaging;
using OneTox.View.UserControls.ProfileSettings;
using OneTox.ViewModel;
using OneTox.ViewModel.Friends;

namespace OneTox.View.Pages
{
    public sealed partial class MainPage : Page
    {
        private readonly MainViewModel _mainViewModel;
        private UserControl _rightPanelContent;

        public MainPage()
        {
            InitializeComponent();

            DataContext = _mainViewModel = (Application.Current as App).MainViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter == null)
            {
                // TODO: Display a splash screen or something if the user doesn't have any friends!
                if (_mainViewModel.FriendList.Friends.Count > 0)
                {
                    FriendList.SelectedItem = _mainViewModel.FriendList.Friends[0];
                    var chatBlock = new ChatBlock {DataContext = _mainViewModel.FriendList.Friends[0]};
                    SetRightPanelContent(chatBlock);
                }
            }
            else if (e.Parameter is FriendViewModel)
            {
                FriendList.SelectedItem = e.Parameter;
                var chatBlock = new ChatBlock {DataContext = e.Parameter};
                SetRightPanelContent(chatBlock);
            }
            else if (Equals(e.Parameter, typeof (SettingsPage)))
            {
                SetRightPanelContent(new ProfileSettingsBlock());
            }
            else if (Equals(e.Parameter, typeof (AddFriendPage)))
            {
                SetRightPanelContent(new AddFriendBlock());
            }
        }

        private void AddFriendButtonClick(object sender, RoutedEventArgs e)
        {
            FriendList.SelectedItem = null;
            SetRightPanelContent(new AddFriendBlock());
        }

        private void FriendListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FriendList.SelectedItem == null)
                return;

            if (_rightPanelContent is ChatBlock)
            {
                _rightPanelContent.DataContext = FriendList.SelectedItem;
            }
            else
            {
                var chatBlock = new ChatBlock {DataContext = FriendList.SelectedItem};
                SetRightPanelContent(chatBlock);
            }
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

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += WindowSizeChanged;
            _mainViewModel.FriendList.Friends.CollectionChanged += FriendsCollectionChangedHandler;
        }

        private void MainPageUnloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= WindowSizeChanged;
            _mainViewModel.FriendList.Friends.CollectionChanged -= FriendsCollectionChangedHandler;
        }

        private void SetRightPanelContent(UserControl userControl)
        {
            RightPanel.Children.Clear();
            RightPanel.Children.Add(userControl);
            _rightPanelContent = userControl;
            VisualStateManager.GoToState(_rightPanelContent, "WideState", false);
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            FriendList.SelectedItem = null;
            SetRightPanelContent(new ProfileSettingsBlock());
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width < 930)
            {
                if (_rightPanelContent is ChatBlock)
                {
                    Frame.Navigate(typeof (ChatPage), FriendList.SelectedItem);
                }
                else if (_rightPanelContent is ProfileSettingsBlock)
                {
                    Frame.Navigate(typeof (SettingsPage));
                }
                else if (_rightPanelContent is AddFriendBlock)
                {
                    Frame.Navigate(typeof (AddFriendPage));
                }
            }
        }
    }
}