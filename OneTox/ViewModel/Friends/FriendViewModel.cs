using OneTox.Common;
using OneTox.Helpers;
using OneTox.Model;
using OneTox.Model.Avatars;
using OneTox.ViewModel.Calls;
using OneTox.ViewModel.FileTransfers;
using OneTox.ViewModel.Messaging;
using SharpTox.Core;
using System;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace OneTox.ViewModel.Friends
{
    public class FriendViewModel : ObservableObject, IToxUserViewModel
    {
        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        private RelayCommand _copyIdCommand;
        private bool _isConnected;
        private string _name;
        private RelayCommand _removeFriendCommand;
        private ExtendedToxUserStatus _status;
        private string _statusMessage;

        public FriendViewModel(int friendNumber)
        {
            FriendNumber = friendNumber;

            Conversation = new ConversationViewModel(this);
            FileTransfers = new FileTransfersViewModel(friendNumber);
            Call = new CallViewModel(friendNumber);

            Name = ToxModel.Instance.GetFriendName(friendNumber);
            if (Name == string.Empty)
            {
                Name = ToxModel.Instance.GetFriendPublicKey(friendNumber).ToString();
            }

            StatusMessage = ToxModel.Instance.GetFriendStatusMessage(friendNumber);
            if (StatusMessage == string.Empty)
            {
                StatusMessage = "Friend request sent.";
            }

            SetFriendStatus(ToxModel.Instance.GetFriendStatus(friendNumber));
            IsConnected = ToxModel.Instance.IsFriendOnline(friendNumber);

            AvatarManager.Instance.FriendAvatarChanged += FriendAvatarChangedHandler;

            ToxModel.Instance.FriendNameChanged += FriendNameChangedHandler;
            ToxModel.Instance.FriendStatusMessageChanged += FriendStatusMessageChangedHandler;
            ToxModel.Instance.FriendStatusChanged += FriendStatusChangedHandler;
            ToxModel.Instance.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
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

        public CallViewModel Call { get; }
        public ConversationViewModel Conversation { get; }

        public RelayCommand CopyIdCommand
        {
            get
            {
                return _copyIdCommand ?? (_copyIdCommand = new RelayCommand(() =>
                {
                    var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
                    dataPackage.SetText(ToxModel.Instance.GetFriendPublicKey(FriendNumber).ToString());
                    Clipboard.SetContent(dataPackage);
                }));
            }
        }

        public FileTransfersViewModel FileTransfers { get; }
        public int FriendNumber { get; }

        public bool IsConnected
        {
            get { return _isConnected; }
            private set
            {
                if (value == _isConnected)
                    return;
                _isConnected = value;
                RaisePropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            private set
            {
                if (value == _name)
                    return;
                _name = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand RemoveFriendCommand
        {
            get
            {
                return _removeFriendCommand
                       ?? (_removeFriendCommand = new RelayCommand(
                           () => { ToxModel.Instance.DeleteFriend(FriendNumber); }));
            }
        }

        public ExtendedToxUserStatus Status
        {
            get { return _status; }
            private set
            {
                if (value == _status)
                    return;
                _status = value;
                RaisePropertyChanged();
            }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set
            {
                if (value == _statusMessage)
                    return;
                _statusMessage = value;
                RaisePropertyChanged();
            }
        }

        private void SetFriendStatus(ToxUserStatus status)
        {
            if (ToxModel.Instance.IsFriendOnline(FriendNumber))
            {
                Status = (ExtendedToxUserStatus)status;
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

        private async void FriendNameChangedHandler(object sender, ToxEventArgs.NameChangeEventArgs e)
        {
            if (FriendNumber != e.FriendNumber)
                return;

            await
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { Name = e.Name; });
        }

        private async void FriendStatusChangedHandler(object sender, ToxEventArgs.StatusEventArgs e)
        {
            if (FriendNumber != e.FriendNumber)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { SetFriendStatus(e.Status); });
        }

        private async void FriendStatusMessageChangedHandler(object sender, ToxEventArgs.StatusMessageEventArgs e)
        {
            if (FriendNumber != e.FriendNumber)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { StatusMessage = e.StatusMessage; });
        }

        #endregion Event handlers
    }
}
