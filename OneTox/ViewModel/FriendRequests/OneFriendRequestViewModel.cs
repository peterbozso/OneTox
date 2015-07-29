using OneTox.Common;
using OneTox.Model;
using SharpTox.Core;

namespace OneTox.ViewModel.FriendRequests
{
    public class OneFriendRequestViewModel
    {
        private readonly FriendRequestsViewModel _friendRequestsViewModel;
        private readonly ToxKey _publicKey;
        private RelayCommand _acceptCommand;
        private RelayCommand _declineCommand;

        public OneFriendRequestViewModel(FriendRequestsViewModel friendRequestsViewModel, ToxKey publicKey,
            string message)
        {
            _friendRequestsViewModel = friendRequestsViewModel;
            _publicKey = publicKey;
            Message = message;
        }

        public string Name
        {
            get { return _publicKey.ToString().Substring(0, 15); }
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
                    _friendRequestsViewModel.Items.Remove(this);
                }));
            }
        }

        public RelayCommand DeclineCommand
        {
            get
            {
                return _declineCommand ??
                       (_declineCommand =
                           new RelayCommand(() => { _friendRequestsViewModel.Items.Remove(this); }));
            }
        }
    }
}