using System;
using SharpTox.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging
{
    public class MessageViewModel : ViewModelBase
    {
        private bool _isDelivered;
        private string _message;

        public MessageViewModel(string message, DateTime timestamp, ToxMessageType messageType, IToxUserViewModel sender,
            int messageId)
        {
            Message = message;
            Timestamp = timestamp;
            MessageType = messageType;
            Sender = sender;
            IsDelivered = sender is FriendViewModel;
            MessageId = messageId;
        }

        public IToxUserViewModel Sender { get; private set; }

        public string Message
        {
            get
            {
                switch (MessageType)
                {
                    case ToxMessageType.Message:
                        return _message;

                    case ToxMessageType.Action:
                        return Sender.Name + " " + _message;
                }
                return null;
            }
            private set { _message = value; }
        }

        public DateTime Timestamp { get; private set; }
        public ToxMessageType MessageType { get; private set; }

        public bool IsDelivered
        {
            get { return _isDelivered; }
            set
            {
                _isDelivered = value;
                RaisePropertyChanged();
            }
        }

        public int MessageId { get; private set; }
    }
}