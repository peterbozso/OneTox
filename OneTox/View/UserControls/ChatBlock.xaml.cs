using System.Threading;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using OneTox.ViewModel.Friends;

namespace OneTox.View.UserControls
{
    public sealed partial class ChatBlock : UserControl
    {
        private readonly Timer _chatTimer;
        private FriendViewModel _friendViewModel;

        public ChatBlock()
        {
            InitializeComponent();

            _chatTimer = new Timer(state => _friendViewModel.Conversation.SetTypingStatus(false),
                null, Timeout.Infinite, Timeout.Infinite);
        }

        public void SetDataContext(FriendViewModel friendViewModel)
        {
            DataContext = friendViewModel;
            _friendViewModel = friendViewModel;
        }

        private async void MessageInputKeyDown(object sender, KeyRoutedEventArgs e)
        {
            _chatTimer.Change(500, -1);
            _friendViewModel.Conversation.SetTypingStatus(true);

            if (e.Key == VirtualKey.Enter && MessageInput.Text != string.Empty)
            {
                // I don't even... 
                // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/734d6c7a-8da2-48c6-9b3d-fa868b4dfb1d/c-textbox-keydown-triggered-twice-in-metro-applications?forum=winappswithcsharp
                if (e.KeyStatus.RepeatCount != 1)
                    return;

                await _friendViewModel.Conversation.SendMessage(MessageInput.Text);
                MessageInput.Text = string.Empty;
                e.Handled = true;
            }
        }
    }
}