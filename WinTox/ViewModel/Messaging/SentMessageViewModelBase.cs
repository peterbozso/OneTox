using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;
using WinTox.Model;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging
{
    public class SentMessageViewModelBase : ToxMessageViewModelBase
    {
        private readonly CoreDispatcher _dispatcher;
        private readonly FriendViewModel _target;

        public SentMessageViewModelBase(string text, DateTime timestamp, ToxMessageType messageType, int id,
            FriendViewModel target)
        {
            Text = text;
            Timestamp = timestamp;
            MessageType = messageType;
            Sender = new UserViewModel();
            IsDelivered = false;
            Id = id;
            _target = target;

            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            ToxModel.Instance.ReadReceiptReceived += ReadReceiptReceivedHandler;
            ToxModel.Instance.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
        }

        public int Id { get; set; }

        private async void ReadReceiptReceivedHandler(object sender, ToxEventArgs.ReadReceiptEventArgs e)
        {
            if (e.FriendNumber != _target.FriendNumber)
                return;

            if (Id == e.Receipt)
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { IsDelivered = true; });
            }
        }

        private void FriendConnectionStatusChangedHandler(object sender, ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (e.FriendNumber != _target.FriendNumber)
                return;

            ResendMessage();
        }

        private void ResendMessage()
        {
            if (!IsDelivered)
            {
                var messageId = ToxModel.Instance.SendMessage(_target.FriendNumber, Text, MessageType);
                Id = messageId; // We have to update the message ID.
            }
        }
    }
}