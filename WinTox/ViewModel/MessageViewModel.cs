using System;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    public class MessageViewModel
    {
        private string _message;

        public MessageViewModel(string message, DateTime timestamp, ToxMessageType messageType, IToxUserViewModel sender)
        {
            Message = message;
            Timestamp = timestamp;
            MessageType = messageType;
            Sender = sender;
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
    }
}