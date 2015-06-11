using System;
using SharpTox.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging
{
    public class ReceivedMessageViewModelBase : ToxMessageViewModelBase
    {
        public ReceivedMessageViewModelBase(string text, DateTime timestamp, ToxMessageType messageType,
            FriendViewModel sender)
        {
            Text = text;
            Timestamp = timestamp;
            MessageType = messageType;
            Sender = sender;
            IsDelivered = true;
        }
    }
}