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
            App.ToxModel.FriendMessageReceived += FriendMessageReceivedHandler;
        }

        private void FriendMessageReceivedHandler(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (e.MessageType == ToxMessageType.Message)
                {
                    Messages.Add(new MessageViewModel
                    {
                        Message = e.Message.Trim(),
                        Timestamp = DateTime.Now.ToString(),
                        UserName = App.ToxModel.GetFriendName(e.FriendNumber)
                    });
                }
            });
        }

        public ObservableCollection<MessageViewModel> Messages { get; set; }
    }
}
