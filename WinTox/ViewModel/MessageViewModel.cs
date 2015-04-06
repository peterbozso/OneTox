using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTox.ViewModel
{
    internal class MessageViewModel
    {
        public string SenderName { get; set; }

        public string Message { get; set; }

        public string Timestamp { get; set; }

        public enum MessageSenderType
        {
            User,
            Friend
        }

        public MessageSenderType SenderType { get; set; }
    }
}
