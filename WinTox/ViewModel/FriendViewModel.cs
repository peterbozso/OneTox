using SharpTox.Core;
using System;
using WinTox.Model;

namespace WinTox.ViewModel
{
    internal class FriendViewModel : ViewModelBase
    {
        public FriendViewModel(int friendNumber)
        {
            FriendNumber = friendNumber;
            Name = ToxSingletonModel.Instance.GetFriendName(friendNumber);
            if (Name == String.Empty)
            {
                Name = ToxSingletonModel.Instance.GetFriendPublicKey(friendNumber).ToString().Substring(0, 10);
            }
            StatusMessage = ToxSingletonModel.Instance.GetFriendStatusMessage(friendNumber);
            if (StatusMessage == String.Empty)
            {
                StatusMessage = "Friend request sent.";
            }
            Status = ToxSingletonModel.Instance.GetFriendStatus(friendNumber);
            IsOnline = ToxSingletonModel.Instance.IsFriendOnline(friendNumber);
        }

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
    }
}
