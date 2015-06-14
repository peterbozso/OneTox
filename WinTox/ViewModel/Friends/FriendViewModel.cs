using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using SharpTox.Core;
using WinTox.Common;
using WinTox.Model;
using WinTox.ViewModel.FileTransfers;
using WinTox.ViewModel.Messaging;

namespace WinTox.ViewModel.Friends
{
    public class FriendViewModel : ViewModelBase, IToxUserViewModel
    {
        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        private bool _isConnected;
        private string _name;
        private RelayCommand _removeFriendCommand;
        private ExtendedToxUserStatus _status;
        private string _statusMessage;

        public FriendViewModel(int friendNumber)
        {
            Conversation = new ConversationViewModel(this);
            FileTransfers = new FileTransfersViewModel(friendNumber);

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

            SetFriendStatus(ToxModel.Instance.GetFriendStatus(FriendNumber));
            IsConnected = ToxModel.Instance.IsFriendOnline(friendNumber);

            AvatarManager.Instance.FriendAvatarChanged += FriendAvatarChangedHandler;

            ToxModel.Instance.FriendNameChanged += FriendNameChangedHandler;
            ToxModel.Instance.FriendStatusMessageChanged += FriendStatusMessageChangedHandler;
            ToxModel.Instance.FriendStatusChanged += FriendStatusChangedHandler;
            ToxModel.Instance.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
        }

        public ConversationViewModel Conversation { get; private set; }
        public FileTransfersViewModel FileTransfers { get; private set; }
        public int FriendNumber { get; private set; }

        public RelayCommand RemoveFriendCommand
        {
            get
            {
                return _removeFriendCommand
                       ?? (_removeFriendCommand = new RelayCommand(
                           () => { ToxModel.Instance.DeleteFriend(FriendNumber); }));
            }
        }

        public BitmapImage Avatar
        {
            get
            {
                if (AvatarManager.Instance.FriendAvatars.ContainsKey(FriendNumber))
                    return AvatarManager.Instance.FriendAvatars[FriendNumber];
                return new BitmapImage(new Uri("ms-appx:///Assets/default-profile-picture.png"));
            }
        }

        public string Name
        {
            get { return _name; }
            private set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set
            {
                _statusMessage = value;
                RaisePropertyChanged();
            }
        }

        public ExtendedToxUserStatus Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            private set
            {
                _isConnected = value;
                RaisePropertyChanged();
            }
        }

        private void SetFriendStatus(ToxUserStatus status)
        {
            if (ToxModel.Instance.IsFriendOnline(FriendNumber))
            {
                Status = (ExtendedToxUserStatus) status;
            }
            else
            {
                Status = ExtendedToxUserStatus.Offline;
            }
        }

        #region Event handlers

        private void FriendAvatarChangedHandler(object sender, int friendNumber)
        {
            if (friendNumber == FriendNumber)
                RaisePropertyChanged("Avatar");
        }

        private async void FriendNameChangedHandler(object sender, ToxEventArgs.NameChangeEventArgs e)
        {
            if (FriendNumber != e.FriendNumber)
                return;

            await
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { Name = e.Name; });
        }

        private async void FriendStatusMessageChangedHandler(object sender, ToxEventArgs.StatusMessageEventArgs e)
        {
            if (FriendNumber != e.FriendNumber)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { StatusMessage = e.StatusMessage; });
        }

        private async void FriendStatusChangedHandler(object sender, ToxEventArgs.StatusEventArgs e)
        {
            if (FriendNumber != e.FriendNumber)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { SetFriendStatus(e.Status); });
        }

        private async void FriendConnectionStatusChangedHandler(object sender,
            ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (FriendNumber != e.FriendNumber)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    IsConnected = e.Status != ToxConnectionStatus.None;
                    SetFriendStatus(ToxModel.Instance.GetFriendStatus(e.FriendNumber));
                });
        }

        #endregion
    }
}