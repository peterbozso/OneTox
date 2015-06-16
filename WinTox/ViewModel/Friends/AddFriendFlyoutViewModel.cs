using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SharpTox.Core;
using WinTox.Common;
using WinTox.Helpers;
using WinTox.Model;

namespace WinTox.ViewModel.Friends
{
    public class AddFriendFlyoutViewModel : ViewModelBase
    {
        private RelayCommand _addFriendCommand;
        private bool _isFlyoutOpen;

        public bool IsFlyoutOpen
        {
            get { return _isFlyoutOpen; }
            set
            {
                _isFlyoutOpen = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand AddFriendCommand
        {
            get
            {
                return _addFriendCommand
                       ?? (_addFriendCommand = new RelayCommand(
                           parameter =>
                           {
                               var flyoutContent = (StackPanel) parameter;

                               var id = GetId(flyoutContent);

                               var toxId = DnsTools.TryDiscoverToxId(id);

                               if (IsIdInvalid(toxId))
                               {
                                   ResetIdTextBox(flyoutContent);
                                   return;
                               }

                               var invitationMessage = GetInvitationMessage(flyoutContent);

                               bool successFulAddd;
                               ToxModel.Instance.AddFriend(new ToxId(toxId), invitationMessage, out successFulAddd);

                               if (successFulAddd)
                               {
                                   ResetFlyout(flyoutContent);
                               }
                           }));
            }
        }

        private string GetId(StackPanel flyoutContent)
        {
            var idTextBox = (TextBox) flyoutContent.FindName("FriendId");
            return idTextBox.Text.Trim();
        }

        private bool IsIdInvalid(string toxId)
        {
            if (toxId.Length < ToxConstants.AddressSize*2)
                return true;

            try
            {
                return !ToxId.IsValid(toxId);
            }
            catch
            {
                return true;
            }
        }

        private void ResetIdTextBox(StackPanel flyoutContent)
        {
            var idTextBox = (TextBox) flyoutContent.FindName("FriendId");
            idTextBox.Text = String.Empty;
            idTextBox.Focus(FocusState.Programmatic);
        }

        private string GetInvitationMessage(StackPanel flyoutContent)
        {
            var invitationMessageTextBox = (TextBox) flyoutContent.FindName("InvitationMessage");
            if (invitationMessageTextBox.Text == String.Empty)
                return "Hello! I'd like to add you to my friends list.";
            return invitationMessageTextBox.Text;
        }

        private void ResetFlyout(StackPanel flyoutContent)
        {
            var idTextBox = (TextBox) flyoutContent.FindName("FriendId");
            idTextBox.Text = String.Empty;

            var invitationMessageTextBox = (TextBox) flyoutContent.FindName("InvitationMessage");
            invitationMessageTextBox.Text = String.Empty;

            IsFlyoutOpen = false;
        }
    }
}