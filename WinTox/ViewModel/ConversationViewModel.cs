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
            if (message.Substring(0, 4).Equals("/me "))
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

        public ObservableCollection<MessageViewModel> Messages { get; set; }
    }
}
