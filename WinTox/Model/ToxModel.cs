using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpTox.Core;

namespace WinTox.Model
{
    public class ToxModel
    {
        private static readonly ToxNode[] _nodes =
        {
            new ToxNode("192.254.75.102", 33445,
                new ToxKey(ToxKeyType.Public, "951C88B7E75C867418ACDB5D273821372BB5BD652740BCDF623A4FA293E75D2F")),
            new ToxNode("144.76.60.215", 33445,
                new ToxKey(ToxKeyType.Public, "04119E835DF3E78BACF0F84235B300546AF8B936F035185E2A8E9E0A67C8924F")),
            new ToxNode("23.226.230.47", 33445,
                new ToxKey(ToxKeyType.Public, "A09162D68618E742FFBCA1C2C70385E6679604B2D80EA6E84AD0996A1AC8A074")),
            new ToxNode("178.62.125.224", 33445,
                new ToxKey(ToxKeyType.Public, "10B20C49ACBD968D7C80F2E8438F92EA51F189F4E70CFBBB2C2C8C799E97F03E")),
            new ToxNode("178.21.112.187", 33445,
                new ToxKey(ToxKeyType.Public, "4B2C19E924972CB9B57732FB172F8A8604DE13EEDA2A6234E348983344B23057")),
            new ToxNode("195.154.119.113 ", 33445,
                new ToxKey(ToxKeyType.Public, "E398A69646B8CEACA9F0B84F553726C1C49270558C57DF5F3C368F05A7D71354")),
            new ToxNode("192.210.149.121", 33445,
                new ToxKey(ToxKeyType.Public, "F404ABAA1C99A9D37D61AB54898F56793E1DEF8BD46B1038B9D822E8460FAB67")),
            new ToxNode("104.219.184.206", 33445,
                new ToxKey(ToxKeyType.Public, "8CD087E31C67568103E8C2A28653337E90E6B8EDA0D765D57C6B5172B4F1F04C"))
        };

        private ExtendedTox _tox;

        public ToxModel()
        {
            var tox = new ExtendedTox(new ToxOptions(true, true))
            {
                Name = "User",
                StatusMessage = "Using WinTox."
            };
            SetCurrent(tox);
        }

        #region Properties

        public int[] Friends
        {
            get { return _tox.Friends; }
        }

        public string UserName
        {
            get { return _tox.Name; }
            set { _tox.Name = value; }
        }

        public string UserStatusMessage
        {
            get { return _tox.StatusMessage; }
        }

        public ToxUserStatus UserStatus
        {
            get { return _tox.Status; }
        }

        public bool IsUserConnected
        {
            get { return _tox.IsConnected; }
        }

        #endregion

        #region Methods

        private void SetCurrent(ExtendedTox tox)
        {
            _tox = tox;

            _tox.OnConnectionStatusChanged += ConnectionStatusChangedHandler;
            _tox.OnFriendListModified += FriendListModifiedHandler;
            _tox.OnFriendRequestReceived += FriendRequestReceivedHandler;
            _tox.OnFriendNameChanged += FriendNameChangedHandler;
            _tox.OnFriendStatusMessageChanged += FriendStatusMessageChangedHandler;
            _tox.OnFriendStatusChanged += FriendStatusChangedHandler;
            _tox.OnFriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
            _tox.OnFriendMessageReceived += FriendMessageReceivedHandler;
            _tox.OnUserDataModified += UserDataModifiedHandler;

            if (FriendListModified != null)
                FriendListModified(-1, ExtendedTox.FriendListModificationType.Reset);
        }

        public void Start()
        {
            _tox.Start();
            BootstrapContinously();
        }

        /// <summary>
        ///     Bootstrap off of 4 random nodes each time until we become connected.
        /// </summary>
        private async void BootstrapContinously()
        {
            var random = new Random();

            while (!_tox.IsConnected)
            {
                for (var i = 0; i < 4; i++)
                {
                    var randomIndex = random.Next(_nodes.Length);
                    _tox.Bootstrap(_nodes[randomIndex]);
                }

                if (!_tox.IsConnected)
                    await Task.Delay(5000);
            }
        }

        public int AddFriend(ToxId id, string message, out ToxErrorFriendAdd error)
        {
            return _tox.AddFriend(id, message, out error);
        }

        public int AddFriendNoRequest(ToxKey publicKey, out ToxErrorFriendAdd error)
        {
            return _tox.AddFriendNoRequest(publicKey, out error);
        }

        public bool DeleteFriend(int friendNumber, out ToxErrorFriendDelete error)
        {
            return _tox.DeleteFriend(friendNumber, out error);
        }

        public string GetFriendName(int friendNumber)
        {
            return _tox.GetFriendName(friendNumber);
        }

        public string GetFriendStatusMessage(int friendNumber)
        {
            return _tox.GetFriendStatusMessage(friendNumber);
        }

        public ToxUserStatus GetFriendStatus(int friendNumber)
        {
            return _tox.GetFriendStatus(friendNumber);
        }

        public bool IsFriendOnline(int friendNumber)
        {
            return _tox.IsFriendOnline(friendNumber);
        }

        public ToxKey GetFriendPublicKey(int friendNumber)
        {
            return _tox.GetFriendPublicKey(friendNumber);
        }

        private const string _fileName = "ToxUserData.tox";

        public async Task SaveDataAsync()
        {
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(_fileName,
                CreationCollisionOption.ReplaceExisting))
            {
                var dataToWrite = _tox.GetData().Bytes;
                await stream.WriteAsync(dataToWrite, 0, dataToWrite.Length);
            }
        }

        public async Task RestoreDataAsync()
        {
            using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(_fileName))
            {
                var toxData = new byte[stream.Length];
                await stream.ReadAsync(toxData, 0, toxData.Length);
                var newTox = new ExtendedTox(new ToxOptions(true, true), ToxData.FromBytes(toxData));
                SetCurrent(newTox);
            }
        }

        public int SendMessage(int friendNumber, string message, ToxMessageType type, out ToxErrorSendMessage error)
        {
            return _tox.SendMessage(friendNumber, message, type, out error);
        }

        #endregion

        #region Events

        public event EventHandler<ToxEventArgs.ConnectionStatusEventArgs> UserConnectionStatusChanged;

        private void ConnectionStatusChangedHandler(object sender, ToxEventArgs.ConnectionStatusEventArgs e)
        {
            if (UserConnectionStatusChanged != null)
                UserConnectionStatusChanged(sender, e);

            if (e.Status == ToxConnectionStatus.None)
                BootstrapContinously();
        }

        public event ExtendedTox.FriendListModifiedEventHandler FriendListModified;

        private async void FriendListModifiedHandler(int friendNumber,
            ExtendedTox.FriendListModificationType modificationType)
        {
            await SaveDataAsync();

            if (FriendListModified != null)
                FriendListModified(friendNumber, modificationType);
        }

        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;

        private void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            if (FriendRequestReceived != null)
                FriendRequestReceived(sender, e);
        }

        public event EventHandler<ToxEventArgs.NameChangeEventArgs> FriendNameChanged;

        private void FriendNameChangedHandler(object sender, ToxEventArgs.NameChangeEventArgs e)
        {
            if (FriendNameChanged != null)
                FriendNameChanged(sender, e);
        }

        public event EventHandler<ToxEventArgs.StatusMessageEventArgs> FriendStatusMessageChanged;

        private void FriendStatusMessageChangedHandler(object sender, ToxEventArgs.StatusMessageEventArgs e)
        {
            if (FriendStatusMessageChanged != null)
                FriendStatusMessageChanged(sender, e);
        }

        public event EventHandler<ToxEventArgs.StatusEventArgs> FriendStatusChanged;

        private void FriendStatusChangedHandler(object sender, ToxEventArgs.StatusEventArgs e)
        {
            if (FriendStatusChanged != null)
                FriendStatusChanged(sender, e);
        }

        public event EventHandler<ToxEventArgs.FriendConnectionStatusEventArgs> FriendConnectionStatusChanged;

        private void FriendConnectionStatusChangedHandler(object sender, ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (FriendConnectionStatusChanged != null)
                FriendConnectionStatusChanged(sender, e);
        }

        public event EventHandler<ToxEventArgs.FriendMessageEventArgs> FriendMessageReceived;

        private void FriendMessageReceivedHandler(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            if (FriendMessageReceived != null)
                FriendMessageReceived(sender, e);
        }

        private async void UserDataModifiedHandler()
        {
            await SaveDataAsync();
        }

        #endregion
    }
}