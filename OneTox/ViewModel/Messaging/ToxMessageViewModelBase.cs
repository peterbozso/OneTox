using System;
using GalaSoft.MvvmLight;
using SharpTox.Core;

namespace OneTox.ViewModel.Messaging
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
        public ToxMessageType MessageType { get; protected set; }
        public IToxUserViewModel Sender { get; protected set; }

        public MessageDeliveryState State
        {
            get { return _state; }
            protected set { Set(ref _state, value); }
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