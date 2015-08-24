using OneTox.ViewModel.Friends;
using SharpTox.Core;
using System;

namespace OneTox.ViewModel.Messaging
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
            State = MessageDeliveryState.Delivered;
        }
    }
}
