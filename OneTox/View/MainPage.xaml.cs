using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OneTox.View.Friends.Controls;
using OneTox.View.Friends.Pages;
using OneTox.View.Messaging.Controls;
using OneTox.View.Messaging.Pages;
using OneTox.View.ProfileSettings.Controls;
using OneTox.View.ProfileSettings.Pages;
using OneTox.ViewModel.Friends;

namespace OneTox.View
{
    public sealed partial class MainPage : Page
    {
        private readonly FriendListViewModel _friendList;
        private UserControl _rightPanelContent;

        public MainPage()
        {
            InitializeComponent();

            _friendList = DataContext as FriendListViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Equals(e.Parameter, typeof (SettingsPage)))
            {
                SetRightPanelContent(new ProfileSettingsBlock());
            }
            else if (Equals(e.Parameter, typeof (AddFriendPage)))
            {
                SetRightPanelContent(new AddFriendBlock());
            }
            else
            {
                // TODO: Display a splash screen or something if the user doesn't have any friends!
                SetRightPanelContent(new ChatBlock());
            }
        }

        private void AddFriendButtonClick(object sender, RoutedEventArgs e)
        {
            _friendList.SelectedFriend = null;
            SetRightPanelContent(new AddFriendBlock());
        }

        private void FriendListItemClick(object sender, ItemClickEventArgs e)
        {
            if (!(_rightPanelContent is ChatBlock))
            {
                SetRightPanelContent(new ChatBlock());
            }
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += WindowSizeChanged;
        }

        private void MainPageUnloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= WindowSizeChanged;
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
            _friendList.SelectedFriend = null;
            SetRightPanelContent(new ProfileSettingsBlock());
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width < 930)
            {
                if (_rightPanelContent is ChatBlock)
                {
                    Frame.Navigate(typeof (ChatPage));
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