using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel {
    internal class MainPageViewModel {
        internal MainPageViewModel() {
            FriendList = new FriendListViewModel();
            ToxSingletonModel.Instance.OnFriendRequestReceived += this.OnFriendRequestReceived;
        }

        public FriendListViewModel FriendList { get; set; }

        internal delegate void FriendRequestReceivedEventHandler(ToxEventArgs.FriendRequestEventArgs e);

        internal event FriendRequestReceivedEventHandler FriendRequestReceived;

        internal void OnFriendRequestReceived(object sender, ToxEventArgs.FriendRequestEventArgs e) {
            if (FriendRequestReceived != null) {
                FriendRequestReceived(e);
            }
        }

        internal enum FriendRequestAnswer {
            Accept,
            Decline,
            Later
        }

        internal void HandleFriendRequestAnswer(FriendRequestAnswer answer, ToxEventArgs.FriendRequestEventArgs e) {
            switch (answer) {
                case FriendRequestAnswer.Accept:
                    ToxSingletonModel.Instance.AddFriendNoRequest(e.PublicKey);
                    return;
                case FriendRequestAnswer.Later:
                    // TODO: Postpone decision!
                    return;
            }
        }
    }
}
