using SharpTox.Core;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using WinTox.Model;

namespace WinTox.ViewModel
{
    internal class FriendListViewModel
    {
        public FriendListViewModel()
        {
            Friends = new ObservableCollection<FriendViewModel>();
            foreach (var friendNumber in ToxSingletonModel.Instance.Friends)
            {
                Friends.Add(new FriendViewModel(friendNumber));
            }

            ToxSingletonModel.Instance.OnFriendNameChanged += this.OnFriendNameChanged;
            ToxSingletonModel.Instance.OnFriendStatusMessageChanged += this.OnFriendStatusMessageChanged;
            ToxSingletonModel.Instance.OnFriendStatusChanged += this.OnFriendStatusChanged;
            ToxSingletonModel.Instance.OnFriendConnectionStatusChanged += this.OnFriendConnectionStatusChanged;
            ToxSingletonModel.Instance.OnFriendListModified += this.OnFriendListModified;
        }

        // We need to run the event handlers from the UI thread.
        // Otherwise the PropertyChanged events wouldn't work in FriendViewModel.

        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

        private void OnFriendNameChanged(object sender, ToxEventArgs.NameChangeEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { FindFriend(e.FriendNumber).Name = e.Name; });
        }

        private void OnFriendStatusMessageChanged(object sender, ToxEventArgs.StatusMessageEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { FindFriend(e.FriendNumber).StatusMessage = e.StatusMessage; });
        }

        private void OnFriendStatusChanged(object sender, ToxEventArgs.StatusEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { FindFriend(e.FriendNumber).Status = e.Status; });
        }

        private void OnFriendConnectionStatusChanged(object sender, ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { FindFriend(e.FriendNumber).IsOnline = e.Status != ToxConnectionStatus.None; });
        }

        private void OnFriendListModified(int friendNumber, ExtendedTox.FriendListModificationType modificationType)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    switch (modificationType)
                    {
                        case ExtendedTox.FriendListModificationType.Add:
                            Friends.Add(new FriendViewModel(friendNumber));
                            return;
                        case ExtendedTox.FriendListModificationType.Delete:
                            Friends.Remove(FindFriend(friendNumber));
                            return;
                    }
                });
        }

        private FriendViewModel FindFriend(int friendNumber)
        {
            foreach (var friend in Friends)
            {
                if (friend.FriendNumber == friendNumber)
                    return friend;
            }
            return null;
        }

        public ObservableCollection<FriendViewModel> Friends { get; set; }
    }
}
