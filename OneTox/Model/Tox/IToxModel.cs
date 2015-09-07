using System;
using System.ComponentModel;
using System.Threading.Tasks;
using SharpTox.Core;
using SharpTox.Encryption;

namespace OneTox.Model.Tox
{
    public interface IToxModel
    {
        #region Properties

        int[] Friends { get; }

        ToxId Id { get; }

        bool IsConnected { get; }

        string Name { get; set; }

        ToxUserStatus Status { get; set; }

        string StatusMessage { get; set; }

        #endregion

        #region Methods

        int AddFriend(ToxId id, string message, out bool success);

        int AddFriendNoRequest(ToxKey Key);

        bool DeleteFriend(int friendNumber);

        bool FileControl(int friendNumber, int fileNumber, ToxFileControl control);

        byte[] FileGetId(int friendNumber, int fileNumber);

        bool FileSeek(int friendNumber, int fileNumber, long position);

        ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName,
            out bool success);

        ToxFileInfo FileSend(int friendNumber, ToxFileKind kind, long fileSize, string fileName, byte[] fileId,
            out bool success);

        bool FileSendChunk(int friendNumber, int fileNumber, long position, byte[] data);

        ToxData GetData();

        ToxData GetData(ToxEncryptionKey key);

        string GetFriendName(int friendNumber);

        ToxKey GetFriendPublicKey(int friendNumber);

        ToxUserStatus GetFriendStatus(int friendNumber);

        string GetFriendStatusMessage(int friendNumber);

        bool IsFriendOnline(int friendNumber);

        ToxConnectionStatus LastConnectionStatusOfFriend(int friendNumber);

        Task RestoreDataAsync();

        Task SaveDataAsync();

        int SendMessage(int friendNumber, string message, ToxMessageType type);

        void SetCurrent(ExtendedTox tox);

        void SetNospam(uint nospam);

        void SetTypingStatus(int friendNumber, bool isTyping);

        void Start();

        #endregion

        #region Events

        event PropertyChangedEventHandler PropertyChanged;

        event EventHandler<ToxEventArgs.FileChunkEventArgs> FileChunkReceived;

        event EventHandler<ToxEventArgs.FileRequestChunkEventArgs> FileChunkRequested;

        event EventHandler<ToxEventArgs.FileControlEventArgs> FileControlReceived;

        event EventHandler<ToxEventArgs.FileSendRequestEventArgs> FileSendRequestReceived;

        event EventHandler<ToxEventArgs.FriendConnectionStatusEventArgs> FriendConnectionStatusChanged;

        event EventHandler<FriendListChangedEventArgs> FriendListChanged;

        event EventHandler<ToxEventArgs.FriendMessageEventArgs> FriendMessageReceived;

        event EventHandler<ToxEventArgs.NameChangeEventArgs> FriendNameChanged;

        event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;

        event EventHandler<ToxEventArgs.StatusEventArgs> FriendStatusChanged;

        event EventHandler<ToxEventArgs.StatusMessageEventArgs> FriendStatusMessageChanged;

        event EventHandler<ToxEventArgs.TypingStatusEventArgs> FriendTypingChanged;

        event EventHandler<ToxEventArgs.ReadReceiptEventArgs> ReadReceiptReceived;

        #endregion
    }
}