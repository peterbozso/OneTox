using System;
using System.Collections.Specialized;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging.RecentMessages
{
    public class RecentMessagesPerUserViewModel : RecentMessagesViewModelBase
    {
        private readonly int _friendNumber;

        public RecentMessagesPerUserViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            RecentMessagesGlobalViewModel.Instace.RecentMessages.CollectionChanged +=
                RecentMessagesGlobalCollectionChangedHandler;
        }

        private async void RecentMessagesGlobalCollectionChangedHandler(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ReceivedMessageViewModel newMessage in e.NewItems)
                {
                    if (((FriendViewModel) newMessage.Sender).FriendNumber != _friendNumber)
                    {
                        await
                            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                TryRemovePreviousMessageFromFriend((FriendViewModel) newMessage.Sender);
                                RecentMessages.Add(newMessage);
                            });
                    }
                }
            }
        }
    }
}