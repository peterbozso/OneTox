using System;
using System.Collections.ObjectModel;
using SharpTox.Core;
using WinTox.Common;
using WinTox.Model;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel
{
    public class MainPageViewModel
    {
        public enum FriendRequestAnswer
        {
            Accept,
            Decline,
            Later
        }

        public MainPageViewModel()
        {
            FriendList = new FriendListViewModel();
            ToxModel.Instance.FriendRequestReceived += FriendRequestReceivedHandler;
            FriendRequests = new ObservableCollection<FriendRequestsViewModel>();
        }

        public FriendListViewModel FriendList { get; set; }
        public ObservableCollection<FriendRequestsViewModel> FriendRequests { get; private set; }

        public void HandleFriendRequestAnswer(FriendRequestAnswer answer, ToxEventArgs.FriendRequestEventArgs e)
        {
            switch (answer)
            {
                case FriendRequestAnswer.Accept:
                    ToxModel.Instance.AddFriendNoRequest(e.PublicKey);
                    return;
                case FriendRequestAnswer.Decline:
                    return;
                case FriendRequestAnswer.Later:
                    FriendRequests.Add(new FriendRequestsViewModel(e.PublicKey, e.Message, FriendRequests));
                    return;
            }
        }

        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;

        private void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            if (FriendRequestReceived != null)
                FriendRequestReceived(sender, e);
        }

        public class FriendRequestsViewModel
        {
            private readonly ObservableCollection<FriendRequestsViewModel> _friendRequests;
            private readonly ToxKey _publicKey;
            private RelayCommand _acceptCommand;
            private RelayCommand _declineCommand;

            public FriendRequestsViewModel(ToxKey publicKey, string message,
                ObservableCollection<FriendRequestsViewModel> friendRequests)
            {
                _publicKey = publicKey;
                Message = message;
                _friendRequests = friendRequests;
            }

            public string Name
            {
                get { return _publicKey.ToString().Substring(0, 20); }
            }

            public string PublicKey
            {
                get { return _publicKey.ToString(); }
            }

            public string Message { get; private set; }

            public RelayCommand AcceptCommand
            {
                get
                {
                    return _acceptCommand ?? (_acceptCommand = new RelayCommand(() =>
                    {
                        ToxModel.Instance.AddFriendNoRequest(_publicKey);
                        _friendRequests.Remove(this);
                    }));
                }
            }

            public RelayCommand DeclineCommand
            {
                get
                {
                    return _declineCommand ??
                           (_declineCommand = new RelayCommand(() => { _friendRequests.Remove(this); }));
                }
            }
        }
    }
}