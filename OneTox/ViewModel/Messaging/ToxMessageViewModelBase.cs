using System;
using OneTox.Helpers;
using SharpTox.Core;

namespace OneTox.ViewModel.Messaging
{
    public enum MessageDeliveryState
    {
        Delivered,
        Pending,
        Failed
    }

    public class ToxMessageViewModelBase : ObservableObject
    {
        private MessageDeliveryState _state;
        private string _text;
        public ToxMessageType MessageType { get; protected set; }
        public IToxUserViewModel Sender { get; protected set; }

        public MessageDeliveryState State
        {
            get { return _state; }
            protected set
            {
                if (value == _state)
                    return;
                _state = value;
                RaisePropertyChanged();
            }
        }

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
            protected set { _text = value; }
        }

        public DateTime Timestamp { get; protected set; }
    }
}