using System;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    internal class MessageViewModel : ViewModelBase
    {
        public enum MessageSenderType
        {
            User,
            Friend
        }

        private string _message;
        private string _senderName;
        private string _timestamp;

        public string SenderName
        {
            get
            {
                switch (MessageType)
                {
                    case ToxMessageType.Message:
                        return _senderName;

                    case ToxMessageType.Action:
                        return _senderName + " " + _message;
                }
                return null;
            }
            set { _senderName = value; }
        }

        public string Message
        {
            get
            {
                switch (MessageType)
                {
                    case ToxMessageType.Message:
                        return _message;

                    case ToxMessageType.Action:
                        return _senderName + " " + _message;
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

        public MessageSenderType SenderType { get; set; }
        public ToxMessageType MessageType { get; set; }
    }
}