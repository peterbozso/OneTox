using SharpTox.Core;
using System;
using WinTox.Common;

namespace WinTox.ViewModel
{
    internal class FriendViewModel : ViewModelBase
    {
        public FriendViewModel(int friendNumber)
        {
            Conversation = new ConversationViewModel();

            FriendNumber = friendNumber;

            Name = App.ToxModel.GetFriendName(friendNumber);
            if (Name == String.Empty)
            {
                Name = App.ToxModel.GetFriendPublicKey(friendNumber).ToString().Substring(0, 10);
            }

            StatusMessage = App.ToxModel.GetFriendStatusMessage(friendNumber);
            if (StatusMessage == String.Empty)
            {
                StatusMessage = "Friend request sent.";
            }

            Status = App.ToxModel.GetFriendStatus(friendNumber);
            IsOnline = App.ToxModel.IsFriendOnline(friendNumber);
        }

        public ConversationViewModel Conversation { get; set; }

        public int FriendNumber { get; set; }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private ToxUserStatus _status;

        public ToxUserStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private bool _isOnline;

        public bool IsOnline
        {
            get { return _isOnline; }
            set
            {
                _isOnline = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _deleteFriendCommand;

        public RelayCommand DeleteFriendCommand
        {
            get
            {
                return _deleteFriendCommand
                       ?? (_deleteFriendCommand = new RelayCommand(
                           (object parameter) =>
                           {
                               ToxErrorFriendDelete error;
                               App.ToxModel.DeleteFriend(FriendNumber, out error);
                               // TODO: Handle errors!!!
                           }));
            }
        }

        public void ReceiveMessage(ToxEventArgs.FriendMessageEventArgs e)
        {
            Conversation.ReceiveMessage(e);
        }
    }
}
