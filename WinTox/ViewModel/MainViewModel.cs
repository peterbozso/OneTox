using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
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

                        var friendIdTextBox = (TextBox)content.FindName("FriendID");
                        var friendId = friendIdTextBox.Text;
                        if (friendId == String.Empty) {
                            friendIdTextBox.Focus(FocusState.Programmatic);
                            return;
                        }

                        var invitationMessage = ((TextBox)content.FindName("InvitationMessage")).Text;
                        if (invitationMessage == String.Empty)
                            invitationMessage = "Hello! I'd like to add you to my friends list.";

                        ToxErrorFriendAdd error;
                        ToxViewModel.Instance.AddFriend(new ToxId(friendId), invitationMessage, out error);
                        // TODO: Handle errors!!!
                    }));
            }
        }
    }
}
