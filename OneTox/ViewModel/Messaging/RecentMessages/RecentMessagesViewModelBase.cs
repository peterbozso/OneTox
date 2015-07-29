using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using OneTox.ViewModel.Friends;

namespace OneTox.ViewModel.Messaging.RecentMessages
{
    public class RecentMessagesViewModelBase
    {
        public RecentMessagesViewModelBase()
        {
            RecentMessages = new ObservableCollection<ReceivedMessageViewModel>();
        }

        public ObservableCollection<ReceivedMessageViewModel> RecentMessages { get; private set; }

        public async Task AddMessage(ReceivedMessageViewModel newMessage)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TryRemovePreviousMessageFromFriend((FriendViewModel) newMessage.Sender);
                RecentMessages.Insert(0, newMessage);
            });
        }

        protected void TryRemovePreviousMessageFromFriend(FriendViewModel friendToSearchFor)
        {
            foreach (var message in RecentMessages)
            {
                var actFriend = (FriendViewModel) message.Sender;
                if (actFriend.FriendNumber == friendToSearchFor.FriendNumber)
                {
                    RecentMessages.Remove(message);
                    return;
                }
            }
        }
    }
}