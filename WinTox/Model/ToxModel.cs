using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SharpTox.Core;
using SharpTox.Encryption;
using WinTox.ViewModel;

namespace WinTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
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

        private static ToxModel _instance;
        private readonly SemaphoreSlim _semaphore;
        private ExtendedTox _tox;

        private Dictionary<int, ToxConnectionStatus> _lastConnectionStatuses; 

        private ToxModel()
        {
            var tox = new ExtendedTox(new ToxOptions(true, true))
            {
                Name = "User",
                StatusMessage = "Using WinTox."
            };
            SetCurrent(tox);

            _semaphore = new SemaphoreSlim(1);

            _lastConnectionStatuses = new Dictionary<int, ToxConnectionStatus>();
        }

        #region Properties

        public static ToxModel Instance
        {
            get { return _instance ?? (_instance = new ToxModel()); }
        }

        public int[] Friends
        {
            get { return _tox.Friends; }
        }

        public string Name
        {
            get { return _tox.Name; }
            set
            {
                _tox.Name = value;
                RaisePropertyChanged();
            }
        }

        public string StatusMessage
        {
            get { return _tox.StatusMessage; }
            set
            {
                _tox.StatusMessage = value;
                RaisePropertyChanged();
            }
        }

        public ToxUserStatus Status
        {
            get { return _tox.Status; }
            set
            {
                _tox.Status = value;
                RaisePropertyChanged();
            }
        }

        public ToxId Id
        {
            get { return _tox.Id; }
        }

        public bool IsConnected
        {
            get { return _tox.IsConnected; }
        }

        #endregion

        #region Methods

        public void SetNospam(uint nospam)
        {
            _tox.SetNospam(nospam);
        }

        /// <summary>
        ///     Replaces the current underlying EntededTox instance with a new one.
        ///     It's used for profile switching.
        /// </summary>
        /// <param name="tox">The new ExtendedTox instance.</param>
        public void SetCurrent(ExtendedTox tox)
        {
            if (_tox != null)
            {
                _tox.Dispose();
            }

            _tox = tox;
            RegisterHandlers();

            RaiseAllPropertiesChanged();
            RaiseFriendListReseted();
        }

        private void RegisterHandlers()
        {
            _tox.OnFriendListChanged += FriendListChangedHandler;
            _tox.OnConnectionStatusChanged += ConnectionStatusChangedHandler;
            _tox.OnFriendRequestReceived += FriendRequestReceivedHandler;
            _tox.OnFriendNameChanged += FriendNameChangedHandler;
            _tox.OnFriendStatusMessageChanged += FriendStatusMessageChangedHandler;
            _tox.OnFriendStatusChanged += FriendStatusChangedHandler;
            _tox.OnFriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
            _tox.OnFriendMessageReceived += FriendMessageReceivedHandler;
            _tox.OnFriendTypingChanged += FriendTypingChangedHandler;
            _tox.OnFileControlReceived += FileControlReceivedHandler;
            _tox.OnFileChunkRequested += FileChunkRequestedHandler;
            _tox.OnFileSendRequestReceived += FileSendRequestReceivedHandler;
            _tox.OnFileChunkReceived += FileChunkReceivedHandler;
            _tox.OnReadReceiptReceived += ReadReceiptReceivedHandler;
        }

        private void RaiseAllPropertiesChanged()
        {
            var properties = typeof (ToxModel).GetRuntimeProperties();
            foreach (var property in properties)
            {
                RaisePropertyChanged(property.Name);
            }
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

        public int AddFriend(ToxId id, string message, out bool success)
        {
            ToxErrorFriendAdd error;
            var retVal = _tox.AddFriend(id, message, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            success = error == ToxErrorFriendAdd.Ok;
            return retVal;
        }

        public int AddFriendNoRequest(ToxKey publicKey)
        {
            ToxErrorFriendAdd error;
            var retVal = _tox.AddFriendNoRequest(publicKey, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public bool DeleteFriend(int friendNumber)
        {
            ToxErrorFriendDelete error;
            var retVal = _tox.DeleteFriend(friendNumber, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
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

        public async Task SaveDataAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync(
                    _tox.Name + ".tox", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(file, _tox.GetData().Bytes);
                ApplicationData.Current.RoamingSettings.Values["currentUserName"] = _tox.Name;
            }
            catch
            {
                // TODO: Exception handling!
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RestoreDataAsync()
        {
            try
            {
                var currentUserName = ApplicationData.Current.RoamingSettings.Values["currentUserName"];
                var file = await ApplicationData.Current.RoamingFolder.GetFileAsync(currentUserName + ".tox");
                var toxData = (await FileIO.ReadBufferAsync(file)).ToArray();
                SetCurrent(new ExtendedTox(new ToxOptions(true, true), ToxData.FromBytes(toxData)));
            }
            catch
            {
                // TODO: Exception handling!
            }
        }

        public ToxData GetData()
        {
            return _tox.GetData();
        }

        public ToxData GetData(ToxEncryptionKey key)
        {
            return _tox.GetData(key);
        }

        public int SendMessage(int friendNumber, string message, ToxMessageType type)
        {
            ToxErrorSendMessage error;
            var retVal = _tox.SendMessage(friendNumber, message, type, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public void SetTypingStatus(int friendNumber, bool isTyping)
        {
            _tox.SetTypingStatus(friendNumber, isTyping);
        }

        public bool FileControl(int friendNumber, int fileNumber, ToxFileControl control)
        {
            ToxErrorFileControl error;
            var retVal = _tox.FileControl(friendNumber, fileNumber, control, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName,
            out bool success)
        {
            ToxErrorFileSend error;
            var retVal = _tox.FileSend(friendNumber, kind, fileSize, fileName, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            success = error == ToxErrorFileSend.Ok;
            return retVal;
        }

        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName, byte[] fileId,
            out bool success)
        {
            ToxErrorFileSend error;
            var retVal = _tox.FileSend(friendNumber, kind, fileSize, fileName, fileId, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            success = error == ToxErrorFileSend.Ok;
            return retVal;
        }

        public bool FileSendChunk(int friendNumber, int fileNumber, long position, byte[] data)
        {
            ToxErrorFileSendChunk error;
            var retVal = _tox.FileSendChunk(friendNumber, fileNumber, position, data, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public byte[] FileGetId(int friendNumber, int fileNumber)
        {
            return _tox.FileGetId(friendNumber, fileNumber);
        }

        public bool FileSeek(int friendNumber, int fileNumber, long position)
        {
            ToxErrorFileSeek error;
            var retVal = _tox.FileSeek(friendNumber, fileNumber, position, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public ToxConnectionStatus LastConnectionStatusOfFriend(int friendNumber)
        {
            if (_lastConnectionStatuses.ContainsKey(friendNumber))
            {
                return _lastConnectionStatuses[friendNumber];
            }

            return ToxConnectionStatus.None;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RaiseFriendListReseted()
        {
            if (FriendListChanged != null)
                FriendListChanged(this,
                    new FriendListChangedEventArgs {FriendNumber = -1, Action = FriendListChangedAction.Reset});
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<FriendListChangedEventArgs> FriendListChanged;

        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;

        public event EventHandler<ToxEventArgs.NameChangeEventArgs> FriendNameChanged;

        public event EventHandler<ToxEventArgs.StatusMessageEventArgs> FriendStatusMessageChanged;

        public event EventHandler<ToxEventArgs.StatusEventArgs> FriendStatusChanged;

        public event EventHandler<ToxEventArgs.FriendConnectionStatusEventArgs> FriendConnectionStatusChanged;

        public event EventHandler<ToxEventArgs.FriendMessageEventArgs> FriendMessageReceived;

        public event EventHandler<ToxEventArgs.TypingStatusEventArgs> FriendTypingChanged;

        public event EventHandler<ToxEventArgs.FileControlEventArgs> FileControlReceived;

        public event EventHandler<ToxEventArgs.FileRequestChunkEventArgs> FileChunkRequested;

        public event EventHandler<ToxEventArgs.FileSendRequestEventArgs> FileSendRequestReceived;

        public event EventHandler<ToxEventArgs.FileChunkEventArgs> FileChunkReceived;

        public event EventHandler<ToxEventArgs.ReadReceiptEventArgs> ReadReceiptReceived;

        #endregion

        #region Event handlers

        private async void FriendListChangedHandler(object sender, FriendListChangedEventArgs e)
        {
            await SaveDataAsync();

            if (FriendListChanged != null)
                FriendListChanged(this, e);
        }

        private void FriendConnectionStatusChangedHandler(object sender, ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (FriendConnectionStatusChanged != null)
                FriendConnectionStatusChanged(this, e);

            _lastConnectionStatuses[e.FriendNumber] = e.Status;
        }

        private void FriendMessageReceivedHandler(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            if (FriendMessageReceived != null)
                FriendMessageReceived(this, e);
        }

        private void ConnectionStatusChangedHandler(object sender, ToxEventArgs.ConnectionStatusEventArgs e)
        {
            RaisePropertyChanged("IsConnected");

            if (e.Status == ToxConnectionStatus.None)
                BootstrapContinously();
        }

        private void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            if (FriendRequestReceived != null)
                FriendRequestReceived(this, e);
        }

        private void FriendNameChangedHandler(object sender, ToxEventArgs.NameChangeEventArgs e)
        {
            if (FriendNameChanged != null)
                FriendNameChanged(this, e);
        }

        private void FriendStatusMessageChangedHandler(object sender, ToxEventArgs.StatusMessageEventArgs e)
        {
            if (FriendStatusMessageChanged != null)
                FriendStatusMessageChanged(this, e);
        }

        private void FriendStatusChangedHandler(object sender, ToxEventArgs.StatusEventArgs e)
        {
            if (FriendStatusChanged != null)
                FriendStatusChanged(this, e);
        }

        private void FriendTypingChangedHandler(object sender, ToxEventArgs.TypingStatusEventArgs e)
        {
            if (FriendTypingChanged != null)
                FriendTypingChanged(this, e);
        }

        private void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            if (FileControlReceived != null)
                FileControlReceived(this, e);
        }

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            if (FileChunkRequested != null)
                FileChunkRequested(this, e);
        }

        private void FileSendRequestReceivedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (FileSendRequestReceived != null)
                FileSendRequestReceived(this, e);
        }

        private void FileChunkReceivedHandler(object sender, ToxEventArgs.FileChunkEventArgs e)
        {
            if (FileChunkReceived != null)
                FileChunkReceived(this, e);
        }

        private void ReadReceiptReceivedHandler(object sender, ToxEventArgs.ReadReceiptEventArgs e)
        {
            if (ReadReceiptReceived != null)
                ReadReceiptReceived(this, e);
        }

        #endregion
    }
}