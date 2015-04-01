using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SharpTox.Core;
using WinTox.Common;
using WinTox.Model;

namespace WinTox.ViewModel {
    class AddFriendFlyoutViewModel : INotifyPropertyChanged {
        private bool _isFlyoutOpen;

        public bool IsFlyoutOpen {
            get { return _isFlyoutOpen; }
            set {
                _isFlyoutOpen = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _addFriendCommand;

        public RelayCommand AddFriendCommand {
            get {
                return _addFriendCommand
                    ?? (_addFriendCommand = new RelayCommand(
                    (object parameter) => {
                        var flyoutContent = (StackPanel)parameter;

                        var friendIdTextBox = (TextBox)flyoutContent.FindName("FriendId");
                        var friendId = friendIdTextBox.Text.Trim();

                        if (!ToxId.IsValid(friendId)) {
                            friendIdTextBox.Text = String.Empty;
                            friendIdTextBox.Focus(FocusState.Programmatic);
                            return;
                        }

                        var invitationMessageTextBox = (TextBox) flyoutContent.FindName("InvitationMessage");
                        var invitationMessage = invitationMessageTextBox.Text;
                        if (invitationMessage == String.Empty)
                            invitationMessage = "Hello! I'd like to add you to my friends list.";

                        ToxErrorFriendAdd error;
                        ToxSingletonModel.Instance.AddFriend(new ToxId(friendId), invitationMessage, out error);
                        // TODO: Handle errors!!!

                        friendIdTextBox.Text = String.Empty;
                        invitationMessageTextBox.Text = String.Empty;
                        IsFlyoutOpen = false;
                    }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
