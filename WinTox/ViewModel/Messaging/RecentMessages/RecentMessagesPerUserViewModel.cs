using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging.RecentMessages
{
    public class RecentMessagesPerUserViewModel
    {
        private readonly int _friendNumber;

        public RecentMessagesPerUserViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            RecentMessages = new ObservableCollection<ReceivedMessageViewModel>();
            RecentMessagesGlobalViewModel.Instace.RecentMessages.CollectionChanged +=
                RecentMessagesGlobalCollectionChangedHandler;
        }

        public ObservableCollection<ReceivedMessageViewModel> RecentMessages { get; private set; }

        private void RecentMessagesGlobalCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ReceivedMessageViewModel newMessage in e.NewItems)
                {
                    if (((FriendViewModel) newMessage.Sender).FriendNumber != _friendNumber)
                    {
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            TryRemovePreviousMessageFromFriend((FriendViewModel) newMessage.Sender);
                            RecentMessages.Add(newMessage);
                        });
                    }
                }
            }
        }

        public void AddMessage(ReceivedMessageViewModel newMessage)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TryRemovePreviousMessageFromFriend((FriendViewModel) newMessage.Sender);
                RecentMessages.Add(newMessage);
            });
        }

        private void TryRemovePreviousMessageFromFriend(FriendViewModel friendToSearchFor)
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