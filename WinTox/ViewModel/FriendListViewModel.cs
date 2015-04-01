using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel {
    class FriendListViewModel {
        public FriendListViewModel() {
            Friends = new ObservableCollection<FriendViewModel>();
            foreach (var friendNumber in ToxSingletonModel.Instance.Friends) {
                Friends.Add(new FriendViewModel(friendNumber));
            }

            ToxSingletonModel.Instance.OnFriendNameChanged += this.OnFriendNameChanged;
            ToxSingletonModel.Instance.OnFriendStatusMessageChanged += this.OnFriendStatusMessageChanged;
            ToxSingletonModel.Instance.OnFriendStatusChanged += this.OnFriendStatusChanged;
            ToxSingletonModel.Instance.OnFriendConnectionStatusChanged += this.OnFriendConnectionStatusChanged;
            ToxSingletonModel.Instance.OnFriendAdded += this.OnFriendAdded;
        }

        // We need to run the event handlers from the UI thread.
        // Otherwise the PropertyChanged events wouldn't work in FriendViewModel.

        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

        private void OnFriendNameChanged(object sender, ToxEventArgs.NameChangeEventArgs e) {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                FindFriend(e.FriendNumber).Name = e.Name;
            });
        }

        private void OnFriendStatusMessageChanged(object sender, ToxEventArgs.StatusMessageEventArgs e) {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                FindFriend(e.FriendNumber).StatusMessage = e.StatusMessage;
            });
        }

        private void OnFriendStatusChanged(object sender, ToxEventArgs.StatusEventArgs e) {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                FindFriend(e.FriendNumber).Status = e.Status;
            });
        }

        private void OnFriendConnectionStatusChanged(object sender, ToxEventArgs.FriendConnectionStatusEventArgs e) {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                FindFriend(e.FriendNumber).IsOnline = e.Status != ToxConnectionStatus.None;
            });
        }

        void OnFriendAdded(int friendNumber) {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                Friends.Add(new FriendViewModel(friendNumber));
            });
        }

        private FriendViewModel FindFriend(int friendNumber) {
            foreach (var friend in Friends) {
                if (friend.FriendNumber == friendNumber)
                    return friend;
            }
            return null;
        }

        public ObservableCollection<FriendViewModel> Friends { get; set; }
    }
}
