using SharpTox.Core;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTox.Common;

namespace WinTox.ViewModel
{
    internal class AddFriendFlyoutViewModel : ViewModelBase
    {
        private bool _isFlyoutOpen;

        public bool IsFlyoutOpen
        {
            get { return _isFlyoutOpen; }
            set
            {
                _isFlyoutOpen = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _addFriendCommand;

        public RelayCommand AddFriendCommand
        {
            get
            {
                return _addFriendCommand
                       ?? (_addFriendCommand = new RelayCommand(
                           (object parameter) =>
                           {
                               var flyoutContent = (StackPanel)parameter;

                               var friendIdTextBox = (TextBox)flyoutContent.FindName("FriendId");
                               var friendId = friendIdTextBox.Text.Trim();

                               if (!ToxId.IsValid(friendId))
                               {
                                   friendIdTextBox.Text = String.Empty;
                                   friendIdTextBox.Focus(FocusState.Programmatic);
                                   return;
                               }

                               var invitationMessageTextBox = (TextBox)flyoutContent.FindName("InvitationMessage");
                               var invitationMessage = invitationMessageTextBox.Text;
                               if (invitationMessage == String.Empty)
                                   invitationMessage = "Hello! I'd like to add you to my friends list.";

                               ToxErrorFriendAdd error;
                               App.ToxModel.AddFriend(new ToxId(friendId), invitationMessage, out error);
                               // TODO: Handle errors!!!

                               friendIdTextBox.Text = String.Empty;
                               invitationMessageTextBox.Text = String.Empty;
                               IsFlyoutOpen = false;
                           }));
            }
        }
    }
}
