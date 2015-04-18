using System;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    internal class MessageViewModel : ViewModelBase
    {
        private string _message;
        private string _timestamp;

        public IToxUserViewModel Sender { get; set; }

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
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public string Timestamp
        {
            get { return _timestamp; }
            set
            {
                _timestamp = value;
                OnPropertyChanged();
            }
        }

        public ToxMessageType MessageType { get; set; }
    }
}