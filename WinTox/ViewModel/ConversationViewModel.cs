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
    public class ConversationViewModel : ViewModelBase
    {
        private readonly FriendViewModel _friendViewModel;

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
        ///     Split a message into ToxConstants.MaxMessageLength long (in bytes) chunks.
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
                if (AppendToLastGroup(message, messageType, sender))
                    return;

                var msgGroup = new MessageGroupViewModel(sender);
                msgGroup.Messages.Add(new MessageViewModel(message, DateTime.Now, messageType, sender));
                MessageGroups.Add(msgGroup);
                OnPropertyChanged("MessageGroups");
            });
        }

        /// <summary>
        ///     Try to append the message to the last message group.
        /// </summary>
        /// <param name="message">The message to append.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="sender">The sender of the message.</param>
        /// <returns>True on success, false otherwise.</returns>
        private bool AppendToLastGroup(string message, ToxMessageType messageType, IToxUserViewModel sender)
        {
            if (MessageGroups.Count == 0 || MessageGroups.Last().Messages.Count == 0)
                return false;

            var lastMessage = MessageGroups.Last().Messages.Last();

            if (lastMessage.Sender.GetType() == sender.GetType())
                // TODO: Implement and use simple equality operator instead.
            {
                MessageGroups.Last().Messages.Add(new MessageViewModel(message, DateTime.Now, messageType, sender));
                OnPropertyChanged("MessageGroups");
                return true;
            }

            return false;
        }
    }
}