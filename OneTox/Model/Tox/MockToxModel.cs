using System;
using System.ComponentModel;
using System.Threading.Tasks;
using SharpTox.Core;
using SharpTox.Encryption;

namespace OneTox.Model.Tox
{
    internal class MockToxModel : IToxModel
    {
        private readonly string[] _statusMessages = new string[10]
        {
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            "Donec a libero sit amet purus pellentesque eleifend quis eu massa.",
            "Integer in mi posuere est efficitur lobortis.",
            "Phasellus sit amet justo varius, ultricies neque at, venenatis diam.",
            "Sed quis sem viverra, volutpat odio eu, pellentesque ex.",
            "Sed sed libero in nulla auctor molestie iaculis vitae neque.",
            "Praesent mattis ipsum quis mauris feugiat, id maximus tellus convallis.",
            "Integer malesuada magna nec dolor sodales, ac consectetur risus aliquet.",
            "In finibus orci nec semper dictum.",
            "Nulla gravida arcu sed porttitor porta."
        };

        public int[] Friends => new int[10] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

        public ToxId Id => new ToxId(new byte[ToxConstants.PublicKeySize], 0);

        public bool IsConnected => true;

        public string Name
        {
            get { return "John Doe"; }
            set { }
        }

        public ToxUserStatus Status
        {
            get { return ToxUserStatus.None; }
            set { }
        }

        public string StatusMessage
        {
            get { return "Designing OneTox."; }
            set { }
        }

        public int AddFriend(ToxId id, string message, out bool success)
        {
            success = true;
            return 0;
        }

        public int AddFriendNoRequest(ToxKey Key)
        {
            return 0;
        }

        public bool DeleteFriend(int friendNumber)
        {
            return true;
        }

        public bool FileControl(int friendNumber, int fileNumber, ToxFileControl control)
        {
            return true;
        }

        public byte[] FileGetId(int friendNumber, int fileNumber)
        {
            return new byte[ToxConstants.FileIdLength];
        }

        public bool FileSeek(int friendNumber, int fileNumber, long position)
        {
            return true;
        }

        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName, out bool success)
        {
            success = true;
            return null;
        }

        public ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName, byte[] fileId,
            out bool success)
        {
            success = true;
            return null;
        }

        public bool FileSendChunk(int friendNumber, int fileNumber, long position, byte[] data)
        {
            return true;
        }

        public ToxData GetData()
        {
            return null;
        }

        public ToxData GetData(ToxEncryptionKey key)
        {
            return null;
        }

        public string GetFriendName(int friendNumber)
        {
            return "Friend #" + friendNumber;
        }

        public ToxKey GetFriendPublicKey(int friendNumber)
        {
            return new ToxKey(ToxKeyType.Public, new byte[ToxConstants.PublicKeySize]);
        }

        public ToxUserStatus GetFriendStatus(int friendNumber)
        {
            return ToxUserStatus.None;
        }

        public string GetFriendStatusMessage(int friendNumber)
        {
            return _statusMessages[friendNumber];
        }

        public bool IsFriendOnline(int friendNumber)
        {
            return true;
        }

        public ToxConnectionStatus LastConnectionStatusOfFriend(int friendNumber)
        {
            return ToxConnectionStatus.None;
        }

        public Task RestoreDataAsync()
        {
            return Task.FromResult(new object());
        }

        public Task SaveDataAsync()
        {
            return Task.FromResult(new object());
        }

        public int SendMessage(int friendNumber, string message, ToxMessageType type)
        {
            return 0;
        }

        public void SetCurrent(ExtendedTox tox)
        {
        }

        public void SetNospam(uint nospam)
        {
        }

        public void SetTypingStatus(int friendNumber, bool isTyping)
        {
        }

        public void Start()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ToxEventArgs.FileChunkEventArgs> FileChunkReceived;
        public event EventHandler<ToxEventArgs.FileRequestChunkEventArgs> FileChunkRequested;
        public event EventHandler<ToxEventArgs.FileControlEventArgs> FileControlReceived;
        public event EventHandler<ToxEventArgs.FileSendRequestEventArgs> FileSendRequestReceived;
        public event EventHandler<ToxEventArgs.FriendConnectionStatusEventArgs> FriendConnectionStatusChanged;
        public event EventHandler<FriendListChangedEventArgs> FriendListChanged;
        public event EventHandler<ToxEventArgs.FriendMessageEventArgs> FriendMessageReceived;
        public event EventHandler<ToxEventArgs.NameChangeEventArgs> FriendNameChanged;
        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;
        public event EventHandler<ToxEventArgs.StatusEventArgs> FriendStatusChanged;
        public event EventHandler<ToxEventArgs.StatusMessageEventArgs> FriendStatusMessageChanged;
        public event EventHandler<ToxEventArgs.TypingStatusEventArgs> FriendTypingChanged;
        public event EventHandler<ToxEventArgs.ReadReceiptEventArgs> ReadReceiptReceived;
    }
}