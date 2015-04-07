using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTox.ViewModel
{
    internal class MessageViewModel
    {
        private string _senderName;

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

        private string _message;

        public string Message
        {
            get
            {
                switch (MessageType)
                {
                    case ToxMessageType.Message:
                        return _message;

                    case ToxMessageType.Action:
                        return String.Empty;
                }
                return null;
            }
            set { _message = value; }
        }

        public string Timestamp { get; set; }

        public enum MessageSenderType
        {
            User,
            Friend
        }

        public MessageSenderType SenderType { get; set; }

        public ToxMessageType MessageType { get; set; }
    }
}
