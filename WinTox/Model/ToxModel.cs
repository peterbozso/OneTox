using SharpTox.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace WinTox.Model
{
    internal class ToxModel
    {
        private static readonly ToxNode[] _nodes =
        {
            new ToxNode("192.254.75.98", 33445,
                new ToxKey(ToxKeyType.Public, "951C88B7E75C867418ACDB5D273821372BB5BD652740BCDF623A4FA293E75D2F")),
            new ToxNode("144.76.60.215", 33445,
                new ToxKey(ToxKeyType.Public, "04119E835DF3E78BACF0F84235B300546AF8B936F035185E2A8E9E0A67C8924F"))
        };

        private ExtendedTox _tox;

        public ToxModel(ExtendedTox tox)
        {
            SetCurrent(tox);
        }

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

            if (FriendListModified != null)
                FriendListModified(-1, ExtendedTox.FriendListModificationType.Reset);
        }

        public void Start()
        {
            foreach (ToxNode node in _nodes)
                _tox.Bootstrap(node);

            _tox.Name = "User";
            _tox.StatusMessage = "This is a test.";

            _tox.Start();

            string id = _tox.Id.ToString();
            Debug.WriteLine("ID: {0}", id);
        }

        #region Properties

        public int[] Friends
        {
            get { return _tox.Friends; }
        }

        public string UserName
        {
            get { return _tox.Name; }
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

        #endregion Properties

        #region Methods

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

        private const string _fileName = "ToxUserData.dat";

        public async Task SaveDataAsync()
        {
            var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(_fileName,
                CreationCollisionOption.ReplaceExisting);
            var dataToWrite = _tox.GetData().Bytes;
            await stream.WriteAsync(dataToWrite, 0, dataToWrite.Length);
        }

        public async Task RestoreDataAsync()
        {
            var stream = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(_fileName);
            byte[] toxData = new byte[stream.Length];
            await stream.ReadAsync(toxData, 0, toxData.Length);
            var newTox = new ExtendedTox(new ToxOptions(true, true), ToxData.FromBytes(toxData));
            SetCurrent(newTox);
        }

        #endregion Methods

        #region Events

        public event EventHandler<ToxEventArgs.ConnectionStatusEventArgs> UserConnectionStatusChanged;

        private void ConnectionStatusChangedHandler(object sender, ToxEventArgs.ConnectionStatusEventArgs e)
        {
            if (UserConnectionStatusChanged != null)
                UserConnectionStatusChanged(sender, e);
        }

        public event ExtendedTox.FriendListModifiedEventHandler FriendListModified;

        private void FriendListModifiedHandler(int friendNumber, ExtendedTox.FriendListModificationType modificationType)
        {
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

        #endregion Events
    }
}
