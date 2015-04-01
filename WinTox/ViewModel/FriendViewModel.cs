using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel {
    class FriendViewModel {
        public FriendViewModel(int friendNumber) {
            FriendNumber = friendNumber;
            Name = ToxSingletonModel.Instance.GetFriendName(friendNumber);
            StatusMessage = ToxSingletonModel.Instance.GetFriendStatusMessage(friendNumber);
            Status = ToxSingletonModel.Instance.GetFriendStatus(friendNumber);
            ConnectionStatus = ToxSingletonModel.Instance.GetFriendConnectionStatus(friendNumber);
        }

        public int FriendNumber { get; set; }

        public string Name { get; set; }

        public string StatusMessage { get; set; }

        public ToxUserStatus Status { get; set; }

        public ToxConnectionStatus ConnectionStatus { get; set; }
    }
}
