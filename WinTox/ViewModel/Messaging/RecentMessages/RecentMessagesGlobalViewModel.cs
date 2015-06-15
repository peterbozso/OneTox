using System.Collections.ObjectModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging.RecentMessages
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class RecentMessagesGlobalViewModel
    {
        private static RecentMessagesGlobalViewModel _instance;

        private RecentMessagesGlobalViewModel()
        {
            RecentMessages = new ObservableCollection<ReceivedMessageViewModel>();
        }

        public static RecentMessagesGlobalViewModel Instace
        {
            get { return _instance ?? (_instance = new RecentMessagesGlobalViewModel()); }
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