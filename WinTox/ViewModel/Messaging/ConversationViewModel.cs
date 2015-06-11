﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        private bool _isFriendTyping;

        public ConversationViewModel(FriendViewModel friendViewModel)
        {
            _friendViewModel = friendViewModel;
            MessageGroups = new ObservableCollection<MessageGroupViewModel>();
            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            ToxModel.Instance.FriendMessageReceived += FriendMessageReceivedHandler;
            ToxModel.Instance.FriendTypingChanged += FriendTypingChangedHandler;
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
            await
                StoreMessage(new ReceivedMessageViewModelBase(e.Message, DateTime.Now, e.MessageType, _friendViewModel));
        }

        public async Task SendMessage(string message)
        {
            var messageType = MessageTools.GetMessageType(message);
            var messageChunks = MessageTools.GetMessageChunks(message, messageType);

            foreach (var chunk in messageChunks)
            {
                var messageId = ToxModel.Instance.SendMessage(_friendViewModel.FriendNumber, chunk, messageType);
                // We store the message with this ID in every case, no matter if the sending was unsuccessful. 
                // If it was, we will resend the message later, and change it's message ID.
                await
                    StoreMessage(new SentMessageViewModelBase(chunk, DateTime.Now, messageType, messageId,
                        _friendViewModel));
            }
        }

        private async Task StoreMessage(ToxMessageViewModelBase message)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (AppendToLastGroup(message))
                    return;

                var msgGroup = new MessageGroupViewModel(message.Sender);
                msgGroup.Messages.Add(message);
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
        private bool AppendToLastGroup(ToxMessageViewModelBase message)
        {
            if (MessageGroups.Count == 0 || MessageGroups.Last().Messages.Count == 0)
                return false;

            var lastMessage = MessageGroups.Last().Messages.Last();

            if (lastMessage.Sender.GetType() == message.Sender.GetType())
                // TODO: Implement and use simple equality operator instead.
            {
                MessageGroups.Last()
                    .Messages.Add(message);
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

        private static class MessageTools
        {
            public static ToxMessageType GetMessageType(string message)
            {
                if (message.Length > 3 && message.Substring(0, 4).Equals("/me "))
                    return ToxMessageType.Action;
                return ToxMessageType.Message;
            }

            public static List<string> GetMessageChunks(string message, ToxMessageType messageType)
            {
                message = TrimMessage(message, messageType);
                return SplitMessage(message);
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
            private static List<string> SplitMessage(string message)
            {
                var messageChunks = new List<string>();

                var encoding = Encoding.UTF8;
                var lengthInBytes = encoding.GetByteCount(message);
                while (lengthInBytes > ToxConstants.MaxMessageLength)
                {
                    var chunk = GetChunkOfMaxMessageLength(message);
                    messageChunks.Add(chunk);
                    message = message.Substring(chunk.Length);
                    lengthInBytes = encoding.GetByteCount(message);
                }
                messageChunks.Add(message);

                return messageChunks;
            }

            // Kudos: http://codereview.stackexchange.com/questions/55103/method-to-return-a-string-of-max-length-in-bytes-vs-characterss
            // TODO: Split messages on new lines or something sensible, not in the middle of words.
            private static string GetChunkOfMaxMessageLength(string input)
            {
                if (string.IsNullOrEmpty(input))
                {
                    return string.Empty;
                }

                var encoding = Encoding.UTF8;
                if (encoding.GetByteCount(input) <= ToxConstants.MaxMessageLength)
                {
                    return input;
                }

                var sb = new StringBuilder();
                var bytes = 0;
                var enumerator = StringInfo.GetTextElementEnumerator(input);
                while (enumerator.MoveNext())
                {
                    var textElement = enumerator.GetTextElement();
                    bytes += encoding.GetByteCount(textElement);
                    if (bytes <= ToxConstants.MaxMessageLength)
                    {
                        sb.Append(textElement);
                    }
                    else
                    {
                        break;
                    }
                }

                return sb.ToString();
            }
        }
    }
}