using System.Collections.ObjectModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class RecentMessagesViewModel
    {
        private static RecentMessagesViewModel _instance;

        private RecentMessagesViewModel()
        {
            RecentMessages = new ObservableCollection<ReceivedMessageViewModel>();
        }

        public static RecentMessagesViewModel Instace
        {
            get { return _instance ?? (_instance = new RecentMessagesViewModel()); }
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