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
            if (e.MessageType == ToxMessageType.Message)
                StoreMessage(e.Message, App.ToxModel.GetFriendName(e.FriendNumber), MessageViewModel.MessageSenderType.Friend);
        }

        public void SendMessage(int friendNumber, string message)
        {
            ToxErrorSendMessage error;
            App.ToxModel.SendMessage(friendNumber, message, ToxMessageType.Message, out error);
            // TODO: Error handling!
            if (error == ToxErrorSendMessage.Ok)
                StoreMessage(message, "me", MessageViewModel.MessageSenderType.User);
        }

        private void StoreMessage(string message, string name, MessageViewModel.MessageSenderType senderType)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Messages.Add(new MessageViewModel
                {
                    Message = message.Trim(),
                    Timestamp = DateTime.Now.ToString(),
                    SenderName = name,
                    SenderType = senderType
                });
            });
        }

        public ObservableCollection<MessageViewModel> Messages { get; set; }
    }
}
