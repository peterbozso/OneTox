using System.Collections.ObjectModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging.RecentMessages
{
    public class RecentMessagesViewModelBase
    {
        public RecentMessagesViewModelBase()
        {
            RecentMessages = new ObservableCollection<ReceivedMessageViewModel>();
        }

        public ObservableCollection<ReceivedMessageViewModel> RecentMessages { get; private set; }

        public void AddMessage(ReceivedMessageViewModel newMessage)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TryRemovePreviousMessageFromFriend((FriendViewModel) newMessage.Sender);
                RecentMessages.Add(newMessage);
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