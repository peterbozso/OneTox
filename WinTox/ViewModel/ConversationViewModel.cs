using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    internal class ConversationViewModel : ViewModelBase
    {
        public ConversationViewModel()
        {
            Messages = new ObservableCollection<MessageViewModel>();
        }

        public ObservableCollection<MessageViewModel> Messages { get; set; }

        public void ReceiveMessage(ToxEventArgs.FriendMessageEventArgs e)
        {
            StoreMessage(e.Message, App.ToxModel.GetFriendName(e.FriendNumber),
                MessageViewModel.MessageSenderType.Friend, e.MessageType);
        }

        public void SendMessage(int friendNumber, string message)
        {
            var messageType = DecideMessageType(message);
            message = TrimMessage(message, messageType);

            var messages = SplitMessage(message);
            foreach (var msg in messages)
            {
                ToxErrorSendMessage error;
                App.ToxModel.SendMessage(friendNumber, msg, messageType, out error);

                // TODO: Error handling!

                if (error == ToxErrorSendMessage.Ok)
                    StoreMessage(msg, App.ToxModel.UserName, MessageViewModel.MessageSenderType.User, messageType);
            }
        }

        private static ToxMessageType DecideMessageType(string message)
        {
            if (message.Length > 3 && message.Substring(0, 4).Equals("/me "))
                return ToxMessageType.Action;
            else
                return ToxMessageType.Message;
        }

        private static string TrimMessage(string message, ToxMessageType messageType)
        {
            if (messageType == ToxMessageType.Action)
                message = message.Remove(0, 4);
            message = message.Trim();
            return message;
        }

        /// <summary>
        /// Split a message into ToxConstants.MaxMessageLength long (in bytes) chunks.
        /// </summary>
        /// <param name="message">The message to split.</param>
        /// <returns>The list of chunks.</returns>
        private List<string> SplitMessage(string message)
        {
            var messages = new List<string>();

            var lengthAsBytes = Encoding.Unicode.GetBytes(message).Length;
            while (lengthAsBytes > ToxConstants.MaxMessageLength)
            {
                var lastSpaceIndex = message.LastIndexOf(" ", ToxConstants.MaxMessageLength, StringComparison.Ordinal);
                var chunk = message.Substring(0, lastSpaceIndex);
                messages.Add(chunk);
                message = message.Substring(lastSpaceIndex + 1);
                lengthAsBytes = Encoding.UTF8.GetBytes(message).Length;
            }
            messages.Add(message);

            return messages;
        }

        private void StoreMessage(string message, string name, MessageViewModel.MessageSenderType senderType,
            ToxMessageType messageType)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (ConcatWithLast(message, senderType, messageType))
                    return;

                Messages.Add(new MessageViewModel
                {
                    Message = message,
                    Timestamp = DateTime.Now.ToString(),
                    SenderName = name,
                    SenderType = senderType,
                    MessageType = messageType
                });
                OnPropertyChanged("Messages");
            });
        }

        /// <summary>
        ///     Try to concatenate the message with the last in the collection.
        /// </summary>
        /// <param name="message">The message to concatenate the last one with.</param>
        /// <param name="senderType">Type of the sender of the message.</param>
        /// <param name="messageType">Type of the message being send.</param>
        /// <returns>True on success, false otherwise.</returns>
        /// TODO: Maybe storing chunks of messages as lists and display a timestamp for every message would be a better (more user friendly) approach of the problem..?
        private bool ConcatWithLast(string message, MessageViewModel.MessageSenderType senderType,
            ToxMessageType messageType)
        {
            if (Messages.Count == 0)
                return false;

            var lastMessage = Messages.Last();
            if (lastMessage.SenderType == senderType && lastMessage.MessageType == ToxMessageType.Message &&
                messageType == ToxMessageType.Message)
            {
                // Concat this message's text to the last one's.
                lastMessage.Message = lastMessage.Message + '\n' + message;
                // Refresh timestamp to be equal to the last message's.
                lastMessage.Timestamp = DateTime.Now.ToString();

                OnPropertyChanged("Messages");

                return true;
            }

            return false;
        }
    }
}