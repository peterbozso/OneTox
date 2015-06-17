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
        private TextBox _invitationMessageTextBox;
        private bool _isFlyoutOpen;
        private TextBox _toxIdTextBox;

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
                               InitializePrivateMembers(parameter);

                               var id = GetId();

                               bool successfulDnsDiscovery;
                               var toxId = DnsTools.TryDiscoverToxId(id, out successfulDnsDiscovery);

                               if (successfulDnsDiscovery)
                               {
                                   SetId(toxId);
                               }
                               else
                               {
                                   toxId = id;

                                   if (!ToxId.IsValid(toxId))
                                   {
                                       ResetIdTextBox();
                                       return;
                                   }

                                   var invitationMessage = GetInvitationMessage();

                                   bool successFulAddd;
                                   ToxModel.Instance.AddFriend(new ToxId(toxId), invitationMessage, out successFulAddd);

                                   if (successFulAddd)
                                   {
                                       ResetFlyout();
                                   }
                               }
                           }));
            }
        }

        /// <summary>
        ///     It must be called before any other private function in this class.
        /// </summary>
        /// <param name="parameter">The StackPanel that serves as the content of the flyout.</param>
        private void InitializePrivateMembers(object parameter)
        {
            var flyoutContent = (StackPanel) parameter;
            _toxIdTextBox = (TextBox) flyoutContent.FindName("FriendId");
            _invitationMessageTextBox = (TextBox) flyoutContent.FindName("InvitationMessage");
        }

        private string GetId()
        {
            return _toxIdTextBox.Text.Trim();
        }

        private void SetId(string newId)
        {
            _toxIdTextBox.Text = newId;
            _toxIdTextBox.Focus(FocusState.Programmatic);
        }

        private void ResetIdTextBox()
        {
            _toxIdTextBox.Text = String.Empty;
            _toxIdTextBox.Focus(FocusState.Programmatic);
        }

        private string GetInvitationMessage()
        {
            if (_invitationMessageTextBox.Text == String.Empty)
                return "Hello! I'd like to add you to my friends list.";
            return _invitationMessageTextBox.Text;
        }

        private void ResetFlyout()
        {
            _toxIdTextBox.Text = String.Empty;

            _invitationMessageTextBox.Text = String.Empty;

            IsFlyoutOpen = false;
        }
    }
}