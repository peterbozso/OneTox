using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace WinTox.ViewModel
{
    internal class ConversationViewModel
    {
        public ConversationViewModel()
        {
            Messages = new ObservableCollection<MessageViewModel>();
        }

        public void ReceiveMessage(ToxEventArgs.FriendMessageEventArgs e)
        {
            StoreMessage(e.Message, App.ToxModel.GetFriendName(e.FriendNumber), MessageViewModel.MessageSenderType.Friend, e.MessageType);
        }

        public void SendMessage(int friendNumber, string message)
        {
            ToxMessageType messageType;
            if (message.Length > 3 && message.Substring(0, 4).Equals("/me "))
            {
                message = message.Remove(0, 4);
                messageType = ToxMessageType.Action;
            }
            else
            {
                messageType = ToxMessageType.Message;
            }

            ToxErrorSendMessage error;
            App.ToxModel.SendMessage(friendNumber, message, messageType, out error);

            // TODO: Error handling!

            if (error == ToxErrorSendMessage.Ok)
                StoreMessage(message, App.ToxModel.UserName, MessageViewModel.MessageSenderType.User, messageType);
        }

        private void StoreMessage(string message, string name, MessageViewModel.MessageSenderType senderType, ToxMessageType messageType)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (ConcatWithLast(message, senderType, messageType))
                    return;

                Messages.Add(new MessageViewModel
                {
                    Message = message.Trim(),
                    Timestamp = DateTime.Now.ToString(),
                    SenderName = name,
                    SenderType = senderType,
                    MessageType = messageType
                });
            });
        }

        /// <summary>
        /// Try to concatenate the message with the last in the collection.
        /// </summary>
        /// <param name="message">The message to concatenate the last one with.</param>
        /// <param name="senderType">Type of the sender of the message.</param>
        /// <param name="messageType">Type of the message being send.</param>
        /// <returns>True on success, false otherwise.</returns>
        /// TODO: Maybe storing chunks of messages as lists and display a timestamp for every message would be a better (more user friendly) approach of the problem..?
        private bool ConcatWithLast(string message, MessageViewModel.MessageSenderType senderType, ToxMessageType messageType)
        {
            if (Messages.Count == 0)
                return false;

            var lastMessage = Messages.Last();
            if (lastMessage.SenderType == senderType && lastMessage.MessageType == ToxMessageType.Message && messageType == ToxMessageType.Message)
            {
                // Concat this message's text to the last one's.
                lastMessage.Message = lastMessage.Message + '\n' + message.Trim();
                // Refresh timestamp to be equal to the last message's.
                lastMessage.Timestamp = DateTime.Now.ToString();
                return true;
            }

            return false;
        }

        public ObservableCollection<MessageViewModel> Messages { get; set; }
    }
}
