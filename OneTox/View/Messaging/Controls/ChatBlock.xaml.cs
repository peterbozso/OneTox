using System.Threading;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using OneTox.ViewModel.Friends;
using OneTox.ViewModel.Messaging;

namespace OneTox.View.Messaging.Controls
{
    public sealed partial class ChatBlock : UserControl
    {
        private readonly Timer _chatTimer;
        private FriendViewModel _friendViewModel;
        private ScrollManager _scrollManager;

        public ChatBlock()
        {
            InitializeComponent();

            _chatTimer = new Timer(state => _friendViewModel.Conversation.SetTypingStatus(false),
                null, Timeout.Infinite, Timeout.Infinite);
        }

        private void ChatBlockDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null)
                return;

            _friendViewModel = DataContext as FriendViewModel;
        }

        private void MessageInputKeyDown(object sender, KeyRoutedEventArgs e)
        {
            _chatTimer.Change(500, -1);
            _friendViewModel.Conversation.SetTypingStatus(true);

            if (e.Key == VirtualKey.Enter && MessageInput.Text != string.Empty)
            {
                // I don't even...
                // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/734d6c7a-8da2-48c6-9b3d-fa868b4dfb1d/c-textbox-keydown-triggered-twice-in-metro-applications?forum=winappswithcsharp
                if (e.KeyStatus.RepeatCount != 1)
                    return;

                _friendViewModel.Conversation.SendMessage(MessageInput.Text);
                MessageInput.Text = string.Empty;
                e.Handled = true;
            }
        }

        private void MessagesListViewLoaded(object sender, RoutedEventArgs e)
        {
            _scrollManager?.DeregisterHandlers();
            _scrollManager = new ScrollManager(MessagesListView, _friendViewModel.Conversation,
                MessageAddedNotificationGrid, MessageAddedNotificationAnimation);
            _scrollManager.RegisterHandlers();
        }

        #region Management of scrolling of MessagesListView

        // TODO: Scroll to the bottom of the conversation when switching between friends!

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
                _messageAddedNotificationGrid.Visibility = Visibility.Collapsed;
            }

            public void DeregisterHandlers()
            {
                _messagesListView.SizeChanged -= MessagesListViewSizeChangedHandler;
                if (_messagesScrollViewer != null)
                {
                    _messagesScrollViewer.ViewChanged -= MessagesScrollViewerViewChangedHandler;
                }
                _conversationViewModel.MessageAdded -= MessageAddedHandler;
                _messageAddedNotificationGrid.Tapped -= MessageAddedNotificationGridTapped;
            }

            public void RegisterHandlers()
            {
                _messagesListView.SizeChanged += MessagesListViewSizeChangedHandler;
                RegsiterMessagesScrollViewerViewChangedHandler();
                _conversationViewModel.MessageAdded += MessageAddedHandler;
                _messageAddedNotificationGrid.Tapped += MessageAddedNotificationGridTapped;
            }

            public void ScrollToBottom(bool disableAnimation)
            {
                _messagesScrollViewer.UpdateLayout();
                _messagesScrollViewer.ChangeView(null, double.MaxValue, null, disableAnimation);
            }

            private ScrollViewer GetScrollViewer(DependencyObject dependencyObject)
            {
                if (dependencyObject is ScrollViewer)
                {
                    return dependencyObject as ScrollViewer;
                }

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);

                    var result = GetScrollViewer(child);
                    if (result == null)
                    {
                        continue;
                    }

                    return result;
                }

                return null;
            }

            private bool IsSticky()
            {
                return (_messagesScrollViewer != null && _stickToBottom);
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

            private void MessagesListViewSizeChangedHandler(object sender, SizeChangedEventArgs e)
            {
                if (IsSticky())
                {
                    ScrollToBottom(true);
                }
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

            private void RegsiterMessagesScrollViewerViewChangedHandler()
            {
                // We need to do it this way because in some cases, if we'd get the ScrollViewer sooner,
                // the function would just return null.
                _messagesScrollViewer = GetScrollViewer(_messagesListView);
                if (_messagesScrollViewer == null)
                {
                    _messagesListView.Loaded += (sender, args) =>
                    {
                        _messagesScrollViewer = GetScrollViewer(_messagesListView);
                        _messagesScrollViewer.ViewChanged += MessagesScrollViewerViewChangedHandler;
                    };
                }
                else
                {
                    _messagesScrollViewer.ViewChanged += MessagesScrollViewerViewChangedHandler;
                }
            }
        }

        #endregion Management of scrolling of MessagesListView
    }
}