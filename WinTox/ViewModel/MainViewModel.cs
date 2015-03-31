using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SharpTox.Core;
using WinTox.Common;

namespace WinTox.ViewModel {
    class MainViewModel {
        private RelayCommand _addFriendCommand;

        public RelayCommand AddFriendCommand {
            get {
                return _addFriendCommand
                    ?? (_addFriendCommand = new RelayCommand(
                    (object parameter) => {
                        var content = (StackPanel)parameter;
                        var friendID = ((TextBox)content.FindName("FriendID")).Text;
                        var invitationMessage = ((TextBox)content.FindName("InvitationMessage")).Text;
                        ToxErrorFriendAdd error;
                        ToxViewModel.Instance.AddFriend(new ToxId(friendID), invitationMessage, out error);
                        // TODO: Handle errors!!!
                    }));
            }
        }
    }
}
