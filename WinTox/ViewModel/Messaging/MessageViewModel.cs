using System;
using SharpTox.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging
{
    public class MessageViewModel : ViewModelBase
    {
        private bool _isDelivered;
        private string _text;

        public MessageViewModel(string text, DateTime timestamp, ToxMessageType messageType, IToxUserViewModel sender,
            int id)
        {
            Text = text;
            Timestamp = timestamp;
            MessageType = messageType;
            Sender = sender;
            IsDelivered = sender is FriendViewModel;
            Id = id;
        }

        public IToxUserViewModel Sender { get; private set; }

        public string Text
        {
            get
            {
                switch (MessageType)
                {
                    case ToxMessageType.Message:
                        return _text;

                    case ToxMessageType.Action:
                        return Sender.Name + " " + _text;
                }
                return null;
            }
            private set { _text = value; }
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

        public int Id { get; private set; }
    }
}