using System;
using SharpTox.Core;
using WinTox.Common;
using WinTox.Helpers;
using WinTox.Model;

namespace WinTox.ViewModel.Friends
{
    public class AddFriendFlyoutViewModel : ViewModelBase
    {
        private RelayCommand _addFriendCommand;
        private string _friendId;
        private string _invitationMessage;
        private bool _isFlyoutClosed;

        public bool IsFlyoutClosed
        {
            get { return _isFlyoutClosed; }
            set
            {
                _isFlyoutClosed = value;
                RaisePropertyChanged();
                if (value)
                    IsFlyoutClosed = false;
            }
        }

        public string FriendId
        {
            get { return _friendId; }
            set
            {
                _friendId = value.Trim();
                RaisePropertyChanged();
            }
        }

        public string InvitationMessage
        {
            get { return _invitationMessage; }
            set
            {
                _invitationMessage = value;
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
                               bool successfulDnsDiscovery;
                               var discoveredToxId = DnsTools.TryDiscoverToxId(FriendId, out successfulDnsDiscovery);

                               if (successfulDnsDiscovery)
                               {
                                   FriendId = discoveredToxId;
                               }
                               else
                               {
                                   if (!ToxId.IsValid(FriendId))
                                   {
                                       FriendId = String.Empty;
                                       // TODO: Tell the user about the problem!
                                       return;
                                   }

                                   var invitationMessage = GetInvitationMessage();

                                   bool successFulAdd;
                                   ToxModel.Instance.AddFriend(new ToxId(FriendId), invitationMessage, out successFulAdd);

                                   if (successFulAdd)
                                   {
                                       ResetFlyout();
                                   }
                               }
                           }));
            }
        }

        private string GetInvitationMessage()
        {
            if (String.IsNullOrEmpty(InvitationMessage))
                return "Hello! I'd like to add you to my friends list.";
            return InvitationMessage;
        }

        private void ResetFlyout()
        {
            FriendId = String.Empty;
            InvitationMessage = String.Empty;
            IsFlyoutClosed = true;
        }
    }
}