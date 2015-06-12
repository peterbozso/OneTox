using System;
using SharpTox.Core;

namespace WinTox.ViewModel.Messaging
{
    public class ToxMessageViewModelBase : ViewModelBase
    {
        private bool _isDelivered;
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

        public bool IsDelivered
        {
            get { return _isDelivered; }
            protected set
            {
                _isDelivered = value;
                RaisePropertyChanged();
            }
        }

        public bool IsFailedToDeliver { get; protected set; }
    }
}