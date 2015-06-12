using System;
using SharpTox.Core;

namespace WinTox.ViewModel.Messaging
{
    public enum MessageDeliveryState
    {
        Delivered,
        Pending,
        Failed
    }

    public class ToxMessageViewModelBase : ViewModelBase
    {
        private MessageDeliveryState _state;
        private string _text;
        public IToxUserViewModel Sender { get; protected set; }

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
        public ToxMessageType MessageType { get; protected set; }

        public MessageDeliveryState State
        {
            get { return _state; }
            protected set
            {
                _state = value;
                RaisePropertyChanged();
            }
        }
    }
}