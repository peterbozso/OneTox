using GalaSoft.MvvmLight.Command;
using OneTox.Model;
using SharpTox.Core;

namespace OneTox.ViewModel.FriendRequests
{
    public class OneFriendRequestViewModel
    {
        private readonly FriendRequestsViewModel _friendRequestsViewModel;
        private readonly ToxKey _publicKey;
        private readonly IToxModel _toxModel;
        private RelayCommand _acceptCommand;
        private RelayCommand _declineCommand;

        public OneFriendRequestViewModel(IToxModel toxModel, FriendRequestsViewModel friendRequestsViewModel,
            ToxKey publicKey,
            string message)
        {
            _toxModel = toxModel;

            _friendRequestsViewModel = friendRequestsViewModel;
            _publicKey = publicKey;
            Message = message;
        }

        public RelayCommand AcceptCommand
        {
            get
            {
                return _acceptCommand ?? (_acceptCommand = new RelayCommand(() =>
                {
                    _toxModel.AddFriendNoRequest(_publicKey);
                    _friendRequestsViewModel.Requests.Remove(this);
                }));
            }
        }

        public RelayCommand DeclineCommand
        {
            get
            {
                return _declineCommand ??
                       (_declineCommand =
                           new RelayCommand(() => { _friendRequestsViewModel.Requests.Remove(this); }));
            }
        }

        public string Message { get; private set; }
        public string PublicKey => _publicKey.ToString();
    }
}