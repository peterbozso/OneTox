using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;
using WinTox.Model;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging
{
    public class ConversationViewModel : ViewModelBase
    {
        private readonly CoreDispatcher _dispatcher;
        private readonly FriendViewModel _friendViewModel;
        private readonly UserViewModel _userViewModel;
        private bool _isFriendTyping;

        public ConversationViewModel(FriendViewModel friendViewModel)
        {
            _friendViewModel = friendViewModel;
            _userViewModel = new UserViewModel();
            MessageGroups = new ObservableCollection<MessageGroupViewModel>();
            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            ToxModel.Instance.FriendMessageReceived += FriendMessageReceivedHandler;
            ToxModel.Instance.FriendTypingChanged += FriendTypingChangedHandler;
            ToxModel.Instance.ReadReceiptReceived += ReadReceiptReceivedHandler;
            ToxModel.Instance.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
        }

        public bool IsFriendTyping
        {
            get { return _isFriendTyping; }
            set
            {
                _isFriendTyping = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MessageGroupViewModel> MessageGroups { get; set; }

        public async Task ReceiveMessage(ToxEventArgs.FriendMessageEventArgs e)
        {
            // Here we make a very benign assumption that message_id stay being uint32_t in toxcore.
            await StoreMessage(e.Message, _friendViewModel, e.MessageType, -1);
        }

        public async Task SendMessage(string message)
        {
            var messageType = DecideMessageType(message);
            message = TrimMessage(message, messageType);

            var messageChunks = SplitMessage(message);
            foreach (var chunk in messageChunks)
            {
                var messageId = ToxModel.Instance.SendMessage(_friendViewModel.FriendNumber, chunk, messageType);
                // We store the message with this ID in every case, no matter if the sending was unsuccessful. 
                // If it was, we will resend the message later, and change it's message ID.
                await StoreMessage(chunk, _userViewModel, messageType, messageId);
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

            var lengthInBytes = Encoding.Unicode.GetByteCount(message);
            while (lengthInBytes > ToxConstants.MaxMessageLength)
            {
                // Division by 2: every character in unicode is 2 bytes long.
                var lastSpaceIndex = message.LastIndexOf(" ", ToxConstants.MaxMessageLength/2, StringComparison.Ordinal);
                var chunk = message.Substring(0, lastSpaceIndex);
                messageChunks.Add(chunk);
                message = message.Substring(lastSpaceIndex + 1);
                lengthInBytes = Encoding.Unicode.GetBytes(message).Length;
            }
            messageChunks.Add(message);

            return messageChunks;
        }

        private async Task StoreMessage(string message, IToxUserViewModel sender,
            ToxMessageType messageType, int messageId)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (AppendToLastGroup(message, messageType, sender, messageId))
                    return;

                var msgGroup = new MessageGroupViewModel(sender);
                msgGroup.Messages.Add(new MessageViewModel(message, DateTime.Now, messageType, sender, messageId));
                MessageGroups.Add(msgGroup);
                RaisePropertyChanged("MessageGroups");
            });
        }

        /// <summary>
        ///     Try to append the message to the last message group.
        /// </summary>
        /// <param name="message">The message to append.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="messageId">The message ID of the message.</param>
        /// <returns>True on success, false otherwise.</returns>
        private bool AppendToLastGroup(string message, ToxMessageType messageType, IToxUserViewModel sender,
            int messageId)
        {
            if (MessageGroups.Count == 0 || MessageGroups.Last().Messages.Count == 0)
                return false;

            var lastMessage = MessageGroups.Last().Messages.Last();

            if (lastMessage.Sender.GetType() == sender.GetType())
                // TODO: Implement and use simple equality operator instead.
            {
                MessageGroups.Last()
                    .Messages.Add(new MessageViewModel(message, DateTime.Now, messageType, sender, messageId));
                RaisePropertyChanged("MessageGroups");
                return true;
            }

            return false;
        }

        public void SetTypingStatus(bool isTyping)
        {
            ToxModel.Instance.SetTypingStatus(_friendViewModel.FriendNumber, isTyping);
        }

        private async void FriendTypingChangedHandler(object sender, ToxEventArgs.TypingStatusEventArgs e)
        {
            if (e.FriendNumber != _friendViewModel.FriendNumber)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { IsFriendTyping = e.IsTyping; });
        }

        private async void FriendMessageReceivedHandler(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            if (e.FriendNumber != _friendViewModel.FriendNumber)
                return;

            await ReceiveMessage(e);
        }

        private async void ReadReceiptReceivedHandler(object sender, ToxEventArgs.ReadReceiptEventArgs e)
        {
            if (e.FriendNumber != _friendViewModel.FriendNumber)
                return;

            var groups = MessageGroups.ToArray();
            foreach (var group in groups)
            {
                var messages = group.Messages.ToArray();
                foreach (var message in messages)
                {
                    if (message.Id == e.Receipt)
                    {
                        await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { message.IsDelivered = true; });
                        return;
                    }
                }
            }
        }

        private void FriendConnectionStatusChangedHandler(object sender, ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (e.FriendNumber != _friendViewModel.FriendNumber)
                return;

            if (ToxModel.Instance.IsFriendOnline(e.FriendNumber))
            {
                // Resend undelivered messages:
                var groups = MessageGroups.ToArray();
                foreach (var group in groups)
                {
                    var messages = group.Messages.ToArray();
                    foreach (var message in messages)
                    {
                        if (!message.IsDelivered)
                        {
                            var messageId = ToxModel.Instance.SendMessage(_friendViewModel.FriendNumber, message.Text,
                                message.MessageType);
                            message.Id = messageId; // We have to update the message ID.
                        }
                    }
                }
            }
        }
    }
}