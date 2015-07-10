using System;
using System.Threading;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinTox.Common;
using WinTox.ViewModel.Friends;
using WinTox.ViewModel.Messaging;

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
        private ScrollManager _scrollManager;

        public ChatPage()
        {
            InitializeComponent();

            NavigationHelper = new NavigationHelper(this);
            NavigationHelper.LoadState += navigationHelper_LoadState;
            NavigationHelper.SaveState += navigationHelper_SaveState;

            _chatTimer = new Timer(state => _friendViewModel.Conversation.SetTypingStatus(false),
                null, Timeout.Infinite, Timeout.Infinite);

            VisualStateManager.GoToState(this, "Default", false);
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

            if (e.Key == VirtualKey.Enter && MessageInputTextBox.Text != String.Empty)
            {
                // I don't even... 
                // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/734d6c7a-8da2-48c6-9b3d-fa868b4dfb1d/c-textbox-keydown-triggered-twice-in-metro-applications?forum=winappswithcsharp
                if (e.KeyStatus.RepeatCount != 1)
                    return;

                await _friendViewModel.Conversation.SendMessage(MessageInputTextBox.Text);
                MessageInputTextBox.Text = String.Empty;
                e.Handled = true;
            }
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

        #region Management of scrolling of MessagesListView

        /// <summary>
        ///     This class's responsibility is to manage the following behavior(s):
        ///     A) (By default and) if the user scrolls to the bottom of the conversation, whenever a new message is received, the
        ///     view scrolls to the bottom to include that message too. It does the same if the size of MessagesListView changes,
        ///     so no matter how long text the user enters to MessageInputTextBox (and by that, automatically increase it's size
        ///     and
        ///     reduce MessagesListView's), the last message would still be shown.
        ///     B) The other case is when the user scrolls up in the conversation. Most likely he/she does it to read previous
        ///     messages. In this case, the user shouldn't be interrupted while reading with the automatically scrolling behavior,
        ///     so it is turned off and the view stays where it is, no matter what happens to the list (a new message is added or
        ///     the TextBox on the bottom grows an squishes it). In this case, we also show a notification what the user can use
        ///     (by clicking/tapping it) to scroll to the bottom instantly.
        /// </summary>
        private class ScrollManager
        {
            private readonly ConversationViewModel _conversationViewModel;
            private readonly Storyboard _messageAddedNotificationAnimation;
            private readonly Grid _messageAddedNotificationGrid;
            private readonly ListView _messagesListView;
            private ScrollViewer _messagesScrollViewer;

            /// <summary>
            ///     If true, we are behaving as defined in A), otherwise B).
            /// </summary>
            private bool _stickToBottom = true;

            public ScrollManager(ListView messagesListView, ConversationViewModel conversationViewModel,
                Grid messageAddedNotificationGrid, Storyboard messageAddedNotificationAnimation)
            {
                _messagesListView = messagesListView;
                _conversationViewModel = conversationViewModel;
                _messageAddedNotificationGrid = messageAddedNotificationGrid;
                _messageAddedNotificationAnimation = messageAddedNotificationAnimation;
            }

            public void RegisterHandlers()
            {
                _messagesListView.SizeChanged += MessagesListViewSizeChangedHandler;
                _messagesListView.Loaded += (sender, args) =>
                {
                    // We need to do it this way because if we'd get the ScrollViewer sooner, the function would just return a null.
                    _messagesScrollViewer = GetScrollViewer(_messagesListView);
                    _messagesScrollViewer.ViewChanged += MessagesScrollViewerViewChangedHandler;
                };
                _conversationViewModel.MessageAdded += MessageAddedHandler;
                _messageAddedNotificationGrid.Tapped += MessageAddedNotificationGridTapped;
            }

            public void DeregisterHandlers()
            {
                _messagesListView.SizeChanged -= MessagesListViewSizeChangedHandler;
                _messagesScrollViewer.ViewChanged -= MessagesScrollViewerViewChangedHandler;
                _conversationViewModel.MessageAdded -= MessageAddedHandler;
                _messageAddedNotificationGrid.Tapped -= MessageAddedNotificationGridTapped;
            }

            private ScrollViewer GetScrollViewer(DependencyObject o)
            {
                if (o is ScrollViewer)
                {
                    return o as ScrollViewer;
                }

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
                {
                    var child = VisualTreeHelper.GetChild(o, i);

                    var result = GetScrollViewer(child);
                    if (result == null)
                    {
                        continue;
                    }

                    return result;
                }

                return null;
            }

            private void MessagesListViewSizeChangedHandler(object sender, SizeChangedEventArgs e)
            {
                if (IsSticky())
                {
                    ScrollToBottom(true);
                }
            }

            private void MessageAddedHandler(object sender, ToxMessageViewModelBase message)
            {
                if (IsSticky())
                {
                    ScrollToBottom(true);
                }
                else
                {
                    // We only play the notification animation if the user receives a message.
                    // We do not disturb the user with his/her own messages, only give him/her the opportunity to
                    // scroll to the bottom fast to catch up with the recent messages.

                    _messageAddedNotificationGrid.Visibility = Visibility.Visible;

                    if (message is ReceivedMessageViewModel)
                    {
                        _messageAddedNotificationAnimation.Begin();
                    }
                }
            }

            private void MessageAddedNotificationGridTapped(object sender, TappedRoutedEventArgs e)
            {
                _messageAddedNotificationAnimation.Stop();
                _messageAddedNotificationGrid.Visibility = Visibility.Collapsed;

                if (_messagesScrollViewer != null)
                {
                    ScrollToBottom(false);
                }
            }

            private bool IsSticky()
            {
                return (_messagesScrollViewer != null && _stickToBottom);
            }

            private void ScrollToBottom(bool disableAnimation)
            {
                _messagesScrollViewer.UpdateLayout();
                _messagesScrollViewer.ChangeView(null, Double.MaxValue, null, disableAnimation);
            }

            private void MessagesScrollViewerViewChangedHandler(object sender, ScrollViewerViewChangedEventArgs e)
            {
                if (e.IsIntermediate)
                    return;

                // We "stick to the bottom" if the user scrolled to the bottom intentionally. Of course it's set true if we scroll
                // to the bottom programmatically as well, but the initial set is always due to the constructor (see case A)), or user activity.
                _stickToBottom = (_messagesScrollViewer.VerticalOffset.Equals(_messagesScrollViewer.ScrollableHeight));

                // If the user scrolled to the bottom manually, we do not show the notification anymore.
                if (_stickToBottom)
                    _messageAddedNotificationGrid.Visibility = Visibility.Collapsed;
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
                DataContext = _friendViewModel = friendViewModel;

                SetupView();
            }
            else
            {
                throw new ArgumentException(
                    "Navigated to ChatPage with wrong type of parameter or with null! An object with the type of FirendViewModel is expected.");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedFrom(e);

            TearDownView();
        }

        private void SetupView()
        {
            _inputPaneChangeHandler = new InputPaneChangeHandler(MessageInputTextBox);
            _inputPaneChangeHandler.RegisterHandlers();

            _scrollManager = new ScrollManager(MessagesListView, _friendViewModel.Conversation,
                MessageAddedNotificationGrid, MessageAddedNotificationAnimation);
            _scrollManager.RegisterHandlers();
        }

        private void TearDownView()
        {
            _inputPaneChangeHandler.DeregisterHandlers();
            _scrollManager.DeregisterHandlers();
        }

        #endregion
    }
}