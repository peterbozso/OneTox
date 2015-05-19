using System;
using SharpTox.Core;
using WinTox.Common;
using WinTox.Model;
using WinTox.ViewModel.Messaging;

namespace WinTox.ViewModel.Friends
{
    public class FriendViewModel : ViewModelBase, IToxUserViewModel
    {
        private bool _isConnected;
        private string _name;
        private RelayCommand _removeFriendCommand;
        private ToxUserStatus _status;
        private string _statusMessage;

        public FriendViewModel(int friendNumber)
        {
            Conversation = new ConversationViewModel(this);

            FriendNumber = friendNumber;

            Name = ToxModel.Instance.GetFriendName(friendNumber);
            if (Name == String.Empty)
            {
                Name = ToxModel.Instance.GetFriendPublicKey(friendNumber).ToString().Substring(0, 10);
            }

            StatusMessage = ToxModel.Instance.GetFriendStatusMessage(friendNumber);
            if (StatusMessage == String.Empty)
            {
                StatusMessage = "Friend request sent.";
            }

            Status = ToxModel.Instance.GetFriendStatus(friendNumber);
            IsConnected = ToxModel.Instance.IsFriendOnline(friendNumber);
        }

        public ConversationViewModel Conversation { get; private set; }
        public int FriendNumber { get; private set; }

        public RelayCommand RemoveFriendCommand
        {
            get
            {
                return _removeFriendCommand
                       ?? (_removeFriendCommand = new RelayCommand(
                           (object parameter) =>
                           {
                               ToxErrorFriendDelete error;
                               ToxModel.Instance.DeleteFriend(FriendNumber, out error);
                               // TODO: Handle errors!!!
                           }));
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                RaisePropertyChanged();
            }
        }

        public ToxUserStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged();
            }
        }

        public void ReceiveMessage(ToxEventArgs.FriendMessageEventArgs e)
        {
            Conversation.ReceiveMessage(e);
        }

        public void SetIsTyping(bool isTyping)
        {
            Conversation.IsFriendTyping = isTyping;
        }
    }
}