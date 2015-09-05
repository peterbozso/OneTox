using System;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using GalaSoft.MvvmLight.Command;
using OneTox.Config;
using OneTox.Helpers;
using OneTox.Model;
using SharpTox.Core;

namespace OneTox.ViewModel.Friends
{
    public class AddFriendViewModel : ObservableObject
    {
        private readonly IToxModel _toxModel;
        private RelayCommand _addFriendCommand;
        private string _friendId;
        private string _friendIdPlaceholder;
        private Timer _friendIdPlaceholderTimer;
        private string _invitationMessage;

        public AddFriendViewModel(IDataService dataService)
        {
            _toxModel = dataService.ToxModel;
        }

        public RelayCommand AddFriendCommand
        {
            get
            {
                return _addFriendCommand
                       ?? (_addFriendCommand = new RelayCommand(
                           () =>
                           {
                               if (string.IsNullOrEmpty(FriendId))
                                   return;

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
                                       ShowMessageAsFriendIdPlaceholder(
                                           "Invalid Tox ID, please enter it more carefully!");
                                       return;
                                   }

                                   var invitationMessage = GetInvitationMessage();

                                   bool successFulAdd;
                                   _toxModel.AddFriend(new ToxId(FriendId), invitationMessage, out successFulAdd);
                                   if (successFulAdd)
                                   {
                                       InvitationMessage = string.Empty;
                                       ShowMessageAsFriendIdPlaceholder("Friend request successfully sent!");
                                   }
                               }
                           }));
            }
        }

        public string FriendId
        {
            get { return _friendId; }
            set
            {
                if (value == _friendId)
                    return;
                _friendId = value.Trim();
                RaisePropertyChanged();
            }
        }

        public string FriendIdPlaceholder
        {
            get { return _friendIdPlaceholder; }
            private set
            {
                if (value == _friendIdPlaceholder)
                    return;
                _friendIdPlaceholder = value;
                RaisePropertyChanged();
            }
        }

        public string InvitationMessage
        {
            get { return _invitationMessage; }
            set
            {
                if (value == _invitationMessage)
                    return;
                _invitationMessage = value;
                RaisePropertyChanged();
            }
        }

        private string GetInvitationMessage()
        {
            if (string.IsNullOrEmpty(InvitationMessage))
                return "Hello! I'd like to add you to my friends list.";
            return InvitationMessage;
        }

        private void ShowMessageAsFriendIdPlaceholder(string message)
        {
            FriendId = string.Empty;
            FriendIdPlaceholder = message;

            if (_friendIdPlaceholderTimer == null)
            {
                _friendIdPlaceholderTimer =
                    new Timer(
                        async state =>
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                () => { FriendIdPlaceholder = string.Empty; }),
                        null, 4500, Timeout.Infinite);
            }
            else
            {
                _friendIdPlaceholderTimer.Change(4500, Timeout.Infinite);
            }
        }
    }
}