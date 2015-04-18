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
        private FriendViewModel _friendViewModel;
        public ConversationViewModel(FriendViewModel friendViewModel)
        {
            _friendViewModel = friendViewModel;
            MessageGroups = new ObservableCollection<MessageGroupViewModel>();
        }

        public ObservableCollection<MessageGroupViewModel> MessageGroups { get; set; }

        public void ReceiveMessage(ToxEventArgs.FriendMessageEventArgs e)
        {
            StoreMessage(e.Message, _friendViewModel, e.MessageType);
        }

        public void SendMessage(string message)
        {
            var messageType = DecideMessageType(message);
            message = TrimMessage(message, messageType);

            var messageChunks = SplitMessage(message);
            foreach (var chunk in messageChunks)
            {
                ToxErrorSendMessage error;
                App.ToxModel.SendMessage(_friendViewModel.FriendNumber, chunk, messageType, out error);

                // TODO: Error handling!

                if (error == ToxErrorSendMessage.Ok)
                    StoreMessage(chunk, App.UserViewModel, messageType);
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
            var messageChunks = new List<string>();

            var lengthAsBytes = Encoding.Unicode.GetBytes(message).Length;
            while (lengthAsBytes > ToxConstants.MaxMessageLength)
            {
                var lastSpaceIndex = message.LastIndexOf(" ", ToxConstants.MaxMessageLength, StringComparison.Ordinal);
                var chunk = message.Substring(0, lastSpaceIndex);
                messageChunks.Add(chunk);
                message = message.Substring(lastSpaceIndex + 1);
                lengthAsBytes = Encoding.UTF8.GetBytes(message).Length;
            }
            messageChunks.Add(message);

            return messageChunks;
        }

        private void StoreMessage(string message, IToxUserViewModel sender,
            ToxMessageType messageType)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (ConcatWithLast(message, messageType, sender))
                    return;

                var msgGroup = new MessageGroupViewModel(sender);
                msgGroup.Messages.Add(new MessageViewModel
                {
                    Message = message,
                    Timestamp = DateTime.Now.ToString(),
                    Sender = sender,
                    MessageType = messageType
                });
                MessageGroups.Add(msgGroup);
                OnPropertyChanged("MessageGroups");
            });
        }

        /// <summary>
        ///     Try to concatenate the message with the last in the collection.
        /// </summary>
        /// <param name="message">The message to concatenate the last one with.</param>
        /// <param name="messageType">Type of the message being send.</param>
        /// <param name="sender">The sender of the message.</param>
        /// <returns>True on success, false otherwise.</returns>
        /// TODO: Maybe storing chunks of messages as lists and display a timestamp for every message would be a better (more user friendly) approach of the problem..?
        private bool ConcatWithLast(string message, ToxMessageType messageType, IToxUserViewModel sender)
        {
            if (MessageGroups.Count == 0 || MessageGroups.Last().Messages.Count == 0)
                return false;

            var lastMessage = MessageGroups.Last().Messages.Last();
            
            if (lastMessage.Sender.GetType() == sender.GetType()) // TODO: Implement and use simple equality operator instead.
            {
                MessageGroups.Last().Messages.Add(new MessageViewModel
                {
                    Message = message,
                    Timestamp = DateTime.Now.ToString(),
                    Sender = sender,
                    MessageType = messageType
                });

                OnPropertyChanged("MessageGroups");

                return true;
            }

            return false;
        }
    }
}