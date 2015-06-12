using System;
using SharpTox.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging
{
    public class ReceivedMessageViewModel : ToxMessageViewModelBase
    {
        public ReceivedMessageViewModel(string text, DateTime timestamp, ToxMessageType messageType,
            FriendViewModel sender)
        {
            Text = text;
            Timestamp = timestamp;
            MessageType = messageType;
            Sender = sender;
            IsDelivered = true;
            IsFailedToDeliver = false;
        }
    }
}