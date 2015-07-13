using SharpTox.Core;
using WinTox.Common;
using WinTox.Model;

namespace WinTox.ViewModel.FriendRequests
{
    public class OneFriendRequestViewModel
    {
        private readonly ToxKey _publicKey;
        private RelayCommand _acceptCommand;
        private RelayCommand _declineCommand;

        public OneFriendRequestViewModel(ToxKey publicKey, string message)
        {
            _publicKey = publicKey;
            Message = message;
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
                    FriendRequestsViewModel.Instance.FriendRequests.Remove(this);
                }));
            }
        }

        public RelayCommand DeclineCommand
        {
            get
            {
                return _declineCommand ??
                       (_declineCommand =
                           new RelayCommand(() => { FriendRequestsViewModel.Instance.FriendRequests.Remove(this); }));
            }
        }
    }
}