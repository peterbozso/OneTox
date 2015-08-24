using OneTox.Model.FileTransfers;
using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OneTox.Model.Avatars
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class AvatarTransferManager
    {
        private static AvatarTransferManager _instance;

        private AvatarTransferManager()
        {
            _transfers = new Dictionary<TransferId, TransferData>();

            ToxModel.Instance.FileControlReceived += FileControlReceivedHandler;
            ToxModel.Instance.FileChunkRequested += FileChunkRequestedHandler;
            ToxModel.Instance.FileSendRequestReceived += FileSendRequestReceivedHandler;
            ToxModel.Instance.FileChunkReceived += FileChunkReceivedHandler;
        }

        public static AvatarTransferManager Instance => _instance ?? (_instance = new AvatarTransferManager());

        #region Data model

        private readonly Dictionary<TransferId, TransferData> _transfers;

        private class TransferData
        {
            private readonly Stream _stream;

            public TransferData(Stream stream, long dataSizeInBytes, TransferDirection direction,
                long transferredBytes = 0)
            {
                _stream = stream;
                if (_stream != null)
                {
                    if (_stream.CanWrite)
                        _stream.SetLength(dataSizeInBytes);

                    _stream.Position = transferredBytes;
                }
                Direction = direction;
            }

            public TransferDirection Direction { get; }

            public void Dispose()
            {
                _stream?.Dispose();
            }

            public MemoryStream GetMemoryStream()
            {
                return _stream as MemoryStream;
            }

            public byte[] GetNextChunk(ToxEventArgs.FileRequestChunkEventArgs e)
            {
                lock (_stream)
                {
                    if (_stream.Position != e.Position)
                    {
                        _stream.Seek(e.Position, SeekOrigin.Begin);
                    }

                    var chunk = new byte[e.Length];
                    _stream.Read(chunk, 0, e.Length);

                    return chunk;
                }
            }

            public bool IsFinished()
            {
                lock (_stream)
                {
                    return _stream.Position == _stream.Length;
                }
            }

            public void PutNextChunk(ToxEventArgs.FileChunkEventArgs e)
            {
                lock (_stream)
                {
                    if (_stream.Position != e.Position)
                    {
                        _stream.Seek(e.Position, SeekOrigin.Begin);
                    }

                    _stream.Write(e.Data, 0, e.Data.Length);
                }
            }
        }

        private class TransferId : IEquatable<TransferId>
        {
            public TransferId(int friendNumber, int fileNumber)
            {
                FriendNumber = friendNumber;
                FileNumber = fileNumber;
            }

            public int FileNumber { get; }
            public int FriendNumber { get; }

            public bool Equals(TransferId other)
            {
                return (FriendNumber == other.FriendNumber) && (FileNumber == other.FileNumber);
            }

            public override int GetHashCode()
            {
                return FriendNumber | (FileNumber << 1);
            }
        }

        #endregion Data model

        #region Common

        private void AddTransfer(int friendNumber, int fileNumber, Stream stream, long dataSizeInBytes,
            TransferDirection direction)
        {
            _transfers.Add(new TransferId(friendNumber, fileNumber),
                new TransferData(stream, dataSizeInBytes, direction));

            Debug.WriteLine(
                "Avatar {0}load added! \t friend number: {1}, \t file number: {2}, \t total avatar transfers: {3}",
                direction, friendNumber, fileNumber, _transfers.Count);
        }

        private void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            Debug.WriteLine("File control received \t friend number: {0}, \t file number: {1}, \t control: {2}",
                e.FriendNumber, e.FileNumber, e.Control);

            var transferId = new TransferId(e.FriendNumber, e.FileNumber);

            if (_transfers.ContainsKey(transferId))
                HandleFileControl(e.Control, transferId);
        }

        private void HandleFileControl(ToxFileControl fileControl, TransferId transferId)
        {
            switch (fileControl)
            {
                case ToxFileControl.Cancel:
                    RemoveTransfer(transferId);
                    return;
            }
        }

        private bool IsTransferFinished(TransferId transferId)
        {
            return !_transfers.ContainsKey(transferId);
        }

        /// <summary>
        ///     By calling this function before sending or receiving an avatar, we ensure that there is only
        ///     1 upload and/or 1 download per friend at the same time.
        /// </summary>
        /// <param name="friendNumber">The friendNumber of the friend we'd like to remove transfers of.</param>
        /// <param name="direction">The direction of the transfers we'd like to remove.</param>
        private void RemoveAllTranfersOfFriendPerDirection(int friendNumber, TransferDirection direction)
        {
            var transfers = _transfers.ToArray();
            foreach (var transfer in transfers)
            {
                if (transfer.Key.FriendNumber == friendNumber && transfer.Value.Direction == direction)
                {
                    SendCancelControl(transfer.Key.FriendNumber, transfer.Key.FileNumber);
                    RemoveTransfer(transfer.Key);
                }
            }
        }

        private void RemoveTransfer(TransferId transferId)
        {
            if (!_transfers.ContainsKey(transferId))
                return;

            var transferToRemove = _transfers[transferId];
            var direction = transferToRemove.Direction;
            transferToRemove.Dispose();
            _transfers.Remove(transferId);

            Debug.WriteLine(
                "Avatar {0}load removed! \t friend number: {1}, \t file number: {2}, \t total avatar transfers: {3}",
                direction, transferId.FriendNumber, transferId.FileNumber, _transfers.Count);
        }

        private void SendCancelControl(int friendNumber, int fileNumber)
        {
            ToxModel.Instance.FileControl(friendNumber, fileNumber, ToxFileControl.Cancel);
        }

        /// <summary>
        ///     Send a ToxFileControl.Resume to the selected friend for the given transfer.
        /// </summary>
        /// <param name="friendNumber">The friend's friend number.</param>
        /// <param name="fileNumber">The file number associated with the transfer.</param>
        /// <returns>True on success, false otherwise.</returns>
        private bool SendResumeControl(int friendNumber, int fileNumber)
        {
            return ToxModel.Instance.FileControl(friendNumber, fileNumber, ToxFileControl.Resume);
        }

        #endregion Common

        #region Sending

        public void SendAvatar(int friendNumber, Stream stream, string fileName)
        {
            RemoveAllTranfersOfFriendPerDirection(friendNumber, TransferDirection.Up);

            bool successfulFileSend;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, stream.Length, fileName,
                GetAvatarHash(stream), out successfulFileSend);

            if (successfulFileSend)
            {
                AddTransfer(friendNumber, fileInfo.Number, stream, stream.Length, TransferDirection.Up);
            }
            else
            {
                stream.Dispose();
            }
        }

        public void SendNullAvatar(int friendNumber)
        {
            bool successfulFileSend;
            ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, 0, "", out successfulFileSend);
        }

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            var transferId = new TransferId(e.FriendNumber, e.FileNumber);

            if (IsTransferFinished(transferId))
                return;

            var currentTransfer = _transfers[transferId];

            var chunk = currentTransfer.GetNextChunk(e);
            var successfulChunkSend = ToxModel.Instance.FileSendChunk(e.FriendNumber, e.FileNumber, e.Position, chunk);

            if (successfulChunkSend)
            {
                if (currentTransfer.IsFinished())
                {
                    HandleFinishedUpload(transferId);
                }
            }
        }

        private byte[] GetAvatarHash(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            return ToxTools.Hash(buffer);
        }

        private void HandleFinishedUpload(TransferId transferId)
        {
            RemoveTransfer(transferId);
        }

        #endregion Sending

        #region Receiving

        private async Task<bool> AlreadyHaveAvatar(int friendNumber, int fileNumber)
        {
            using (var stream = await AvatarManager.Instance.GetFriendAvatarStream(friendNumber))
            {
                if (stream == null)
                    return false;
                var fileId = ToxModel.Instance.FileGetId(friendNumber, fileNumber);
                var avatarHash = GetAvatarHash(stream);
                return fileId.SequenceEqual(avatarHash);
            }
        }

        private void FileChunkReceivedHandler(object sender, ToxEventArgs.FileChunkEventArgs e)
        {
            var transferId = new TransferId(e.FriendNumber, e.FileNumber);

            if (IsTransferFinished(transferId))
                return;

            var currentTransfer = _transfers[transferId];

            currentTransfer.PutNextChunk(e);

            if (currentTransfer.IsFinished())
            {
                HandleFinishedDownload(transferId, e.FriendNumber, e.FileNumber);
            }
        }

        private async void FileSendRequestReceivedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FileKind != ToxFileKind.Avatar)
                return;

            if (e.FileSize == 0) // It means the avatar of the friend is removed.
            {
                SendCancelControl(e.FriendNumber, e.FileNumber);
                await AvatarManager.Instance.RemoveFriendAvatar(e.FriendNumber);
                return;
            }

            if (await AlreadyHaveAvatar(e.FriendNumber, e.FileNumber))
            {
                SendCancelControl(e.FriendNumber, e.FileNumber);
                return;
            }

            RemoveAllTranfersOfFriendPerDirection(e.FriendNumber, TransferDirection.Down);

            var resumeSent = SendResumeControl(e.FriendNumber, e.FileNumber);
            if (resumeSent)
            {
                var stream = new MemoryStream((int)e.FileSize);
                AddTransfer(e.FriendNumber, e.FileNumber, stream, e.FileSize, TransferDirection.Down);
            }
        }

        private void HandleFinishedDownload(TransferId transferId, int friendNumber, int fileNumber)
        {
            AvatarManager.Instance.ChangeFriendAvatar(friendNumber,
                (_transfers[transferId]).GetMemoryStream());
            RemoveTransfer(transferId);
        }

        #endregion Receiving
    }
}
