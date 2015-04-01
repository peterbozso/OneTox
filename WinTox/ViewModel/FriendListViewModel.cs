using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel {
    class FriendListViewModel {
        internal FriendListViewModel() {
            FriendList = new ObservableCollection<FriendViewModel>();
            foreach (var friendNumber in ToxSingletonModel.Instance.Friends) {
                FriendList.Add(new FriendViewModel(friendNumber));
            }

            ToxSingletonModel.Instance.OnFriendNameChanged += this.OnFriendNameChanged;
            ToxSingletonModel.Instance.OnFriendStatusMessageChanged += this.OnFriendStatusMessageChanged;
            ToxSingletonModel.Instance.OnFriendStatusChanged += this.OnFriendStatusChanged;
            ToxSingletonModel.Instance.OnFriendConnectionStatusChanged += this.OnFriendConnectionStatusChanged;
        }

        private void OnFriendNameChanged(object sender, SharpTox.Core.ToxEventArgs.NameChangeEventArgs e) {
            FindFriend(e.FriendNumber).Name = e.Name;
        }

        private void OnFriendStatusMessageChanged(object sender, SharpTox.Core.ToxEventArgs.StatusMessageEventArgs e) {
            FindFriend(e.FriendNumber).StatusMessage = e.StatusMessage;
        }

        private void OnFriendStatusChanged(object sender, SharpTox.Core.ToxEventArgs.StatusEventArgs e) {
            FindFriend(e.FriendNumber).Status = e.Status;
        }

        private void OnFriendConnectionStatusChanged(object sender, SharpTox.Core.ToxEventArgs.FriendConnectionStatusEventArgs e) {
            FindFriend(e.FriendNumber).IsOnline = e.Status != ToxConnectionStatus.None;
        }

        private FriendViewModel FindFriend(int friendNumber) {
            foreach (var friend in FriendList) {
                if (friend.FriendNumber == friendNumber)
                    return friend;
            }
            return null;
        }

        internal ObservableCollection<FriendViewModel> FriendList { get; set; } 
    }
}
