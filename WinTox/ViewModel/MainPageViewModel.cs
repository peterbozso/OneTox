using System.Collections.Specialized;
using Windows.UI.Xaml;
using WinTox.Helpers;
using WinTox.ViewModel.FriendRequests;
using WinTox.ViewModel.Friends;
using WinTox.ViewModel.Messaging.RecentMessages;

namespace WinTox.ViewModel
{
    public class MainPageViewModel : ObservableObject
    {
        private Visibility _friendRequestsListVisibility;
        private Visibility _recentMessagesListVisibility;

        public MainPageViewModel()
        {
            FriendList = new FriendListViewModel();
            DecideFriendRequestsListVisibility();
            DecideRecentMessagesListVisiblity();
            RecentMessagesGlobalViewModel.Instace.RecentMessages.CollectionChanged +=
                RecentMessagesCollectionChangedHandler;
            FriendRequestsViewModel.Instance.FriendRequests.CollectionChanged += FriendRequestsCollectionChangedHandler;
        }

        public FriendListViewModel FriendList { get; set; }

        public Visibility RecentMessagesListVisibility
        {
            get { return _recentMessagesListVisibility; }
            set
            {
                if (value == _recentMessagesListVisibility)
                    return;
                _recentMessagesListVisibility = value;
                RaisePropertyChanged();
            }
        }

        public Visibility FriendRequestsListVisibility
        {
            get { return _friendRequestsListVisibility; }
            set
            {
                if (value == _friendRequestsListVisibility)
                    return;
                _friendRequestsListVisibility = value;
                RaisePropertyChanged();
            }
        }

        private void FriendRequestsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            DecideFriendRequestsListVisibility();
        }

        private void DecideFriendRequestsListVisibility()
        {
            FriendRequestsListVisibility = FriendRequestsViewModel.Instance.FriendRequests.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void RecentMessagesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            DecideRecentMessagesListVisiblity();
        }

        private void DecideRecentMessagesListVisiblity()
        {
            RecentMessagesListVisibility = RecentMessagesGlobalViewModel.Instace.RecentMessages.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}