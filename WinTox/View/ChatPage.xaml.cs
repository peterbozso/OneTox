using System;
using System.Threading;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using WinTox.Common;
using WinTox.ViewModel.Friends;

namespace WinTox.View
{
    /// <summary>
    ///     A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        private readonly Timer _chatTimer;
        private FriendViewModel _friendViewModel;
        private InputPaneChangeHandler _inputPaneChangeHandler;

        public ChatPage()
        {
            InitializeComponent();

            NavigationHelper = new NavigationHelper(this);
            NavigationHelper.LoadState += navigationHelper_LoadState;
            NavigationHelper.SaveState += navigationHelper_SaveState;

            _chatTimer = new Timer(state => _friendViewModel.Conversation.SetTypingStatus(false),
                null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        ///     NavigationHelper is used on each page to aid in navigation and
        ///     process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper { get; private set; }

        /// <summary>
        ///     Populates the page with content passed during navigation. Any saved state is also
        ///     provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        ///     The source of the event; typically <see cref="NavigationHelper" />
        /// </param>
        /// <param name="e">
        ///     Event data that provides both the navigation parameter passed to
        ///     <see cref="Frame.Navigate(Type, Object)" /> when this page was initially requested and
        ///     a dictionary of state preserved by this page during an earlier
        ///     session. The state will be null the first time a page is visited.
        /// </param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        ///     Preserves state associated with this page in case the application is suspended or the
        ///     page is discarded from the navigation cache.  Values must conform to the serialization
        ///     requirements of <see cref="SuspensionManager.SessionState" />.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper" /></param>
        /// <param name="e">
        ///     Event data that provides an empty dictionary to be populated with
        ///     serializable state.
        /// </param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private async void MessageInputKeyDown(object sender, KeyRoutedEventArgs e)
        {
            _chatTimer.Change(500, -1);
            _friendViewModel.Conversation.SetTypingStatus(true);

            if (e.Key == VirtualKey.Enter && MessageInput.Text != String.Empty)
            {
                // I don't even... 
                // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/734d6c7a-8da2-48c6-9b3d-fa868b4dfb1d/c-textbox-keydown-triggered-twice-in-metro-applications?forum=winappswithcsharp
                if (e.KeyStatus.RepeatCount != 1)
                    return;

                await _friendViewModel.Conversation.SendMessage(MessageInput.Text);
                MessageInput.Text = String.Empty;
                e.Handled = true;
            }
        }

        private void SetupViewModel(FriendViewModel friendViewModel)
        {
            DataContext = _friendViewModel = friendViewModel;
            _friendViewModel.Conversation.MessageAdded += MessageAddedHandler;
        }

        private void MessageAddedHandler(object sender, EventArgs e)
        {
            var selectedIndex = MessagesListView.Items.Count - 1;
            MessagesListView.SelectedIndex = selectedIndex;
            MessagesListView.UpdateLayout();
            MessagesListView.ScrollIntoView(MessagesListView.SelectedItem);
        }

        private async void SendFileButtonClick(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add("*");

            var files = await openPicker.PickMultipleFilesAsync();
            if (files.Count == 0)
                return;

            foreach (var file in files)
            {
                await _friendViewModel.FileTransfers.SendFile(file);
            }
        }

        #region Handle changes of the input pane's state

        /// <summary>
        ///     Inner class for handling the input pane's changes (the on-screen keyboard is being shown/hidden).
        /// </summary>
        private class InputPaneChangeHandler
        {
            private readonly InputPane _inputPane = InputPane.GetForCurrentView();
            private readonly TextBox _messageInputTextBox;

            public InputPaneChangeHandler(TextBox messageInputTextBox)
            {
                _messageInputTextBox = messageInputTextBox;
            }

            public void RegisterHandlers()
            {
                _inputPane.Showing += ShowingHandler;
                _inputPane.Hiding += HidingHandler;
            }

            public void DeregisterHandlers()
            {
                _inputPane.Showing -= ShowingHandler;
                _inputPane.Hiding -= HidingHandler;
            }

            private void ShowingHandler(InputPane sender, InputPaneVisibilityEventArgs args)
            {
                args.EnsuredFocusedElementInView = true;
                _messageInputTextBox.Margin = new Thickness(20, 20, 20, args.OccludedRect.Height + 20);
            }

            private void HidingHandler(InputPane sender, InputPaneVisibilityEventArgs args)
            {
                _messageInputTextBox.Margin = new Thickness(20);
            }
        }

        #endregion

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="Common.NavigationHelper.LoadState" />
        /// and
        /// <see cref="Common.NavigationHelper.SaveState" />
        /// .
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedTo(e);

            var friendViewModel = e.Parameter as FriendViewModel;
            if (friendViewModel != null)
            {
                SetupViewModel(friendViewModel);
            }

            _inputPaneChangeHandler = new InputPaneChangeHandler(MessageInput);
            _inputPaneChangeHandler.RegisterHandlers();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedFrom(e);

            _inputPaneChangeHandler.DeregisterHandlers();
        }

        #endregion
    }
}