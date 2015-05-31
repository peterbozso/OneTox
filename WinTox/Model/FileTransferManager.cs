using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpTox.Core;

namespace WinTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class FileTransferManager
    {
        private static FileTransferManager _instance;
        private readonly Dictionary<TransferId, TransferData> _activeTransfers;

        private FileTransferManager()
        {
            _activeTransfers = new Dictionary<TransferId, TransferData>();

            ToxModel.Instance.FileControlReceived += FileControlReceivedHandler;
            ToxModel.Instance.FileChunkReceived += FileChunkReceivedHandler;
            ToxModel.Instance.FileSendRequestReceived += FileSendRequestReceivedHandler;
            ToxModel.Instance.FileChunkRequested += FileChunkRequestedHandler;
        }

        public static FileTransferManager Instance
        {
            get { return _instance ?? (_instance = new FileTransferManager()); }
        }

        private void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            switch (e.Control)
            {
                case ToxFileControl.Cancel:
                    var transferId = new TransferId(e.FileNumber, e.FriendNumber);
                    if (_activeTransfers.ContainsKey(transferId))
                    {
                        _activeTransfers.Remove(transferId);
                        Debug.WriteLine(
                            "File transfer CANCELLED! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                            e.FriendNumber, e.FileNumber, _activeTransfers.Count);
                    }
                    return;
            }
        }

        private bool IsTransferFinished(TransferId transferId)
        {
            return !_activeTransfers.ContainsKey(transferId);
        }

        #region Sending

        public async Task SendAvatar(int friendNumber, StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();

            ToxErrorFileSend error;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, stream.Length, file.Name,
                GetAvatarHash(stream), out error);

            if (error == ToxErrorFileSend.Ok)
            {
                _activeTransfers.Add(new TransferId(fileInfo.Number, friendNumber),
                    new TransferData(ToxFileKind.Avatar, stream, stream.Length));
                Debug.WriteLine(
                    "File upload added! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                    friendNumber, fileInfo.Number, _activeTransfers.Count);
            }
            // TODO: Error handling!
        }

        public void SendNullAvatar(int friendNumber)
        {
            ToxErrorFileSend error;
            ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, 0, "", GetAvatarHash(new MemoryStream()),
                out error);
            // TODO: Error handling!
        }

        private byte[] GetAvatarHash(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            return ToxTools.Hash(buffer);
        }

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            var transferId = new TransferId(e.FileNumber, e.FriendNumber);
            if (IsTransferFinished(transferId))
                return;

            var currentTransfer = _activeTransfers[transferId];

            var chunk = GetNextChunk(e, currentTransfer);
            ToxErrorFileSendChunk error;
            ToxModel.Instance.FileSendChunk(e.FriendNumber, e.FileNumber, e.Position, chunk, out error);
            if (error == ToxErrorFileSendChunk.Ok)
            {
                currentTransfer.IncreaseProgress(e.Length);
                if (currentTransfer.IsFinished())
                {
                    _activeTransfers.Remove(transferId);
                    Debug.WriteLine(
                        "File upload removed! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                        e.FriendNumber, e.FileNumber, _activeTransfers.Count);
                }
            }
            // TODO: Error handling!
        }

        private byte[] GetNextChunk(ToxEventArgs.FileRequestChunkEventArgs e, TransferData currentTransfer)
        {
            var currentStream = currentTransfer.Stream;
            if (e.Position != currentStream.Position)
                currentStream.Seek(e.Position, SeekOrigin.Begin);
            var chunk = new byte[e.Length];
            currentStream.Read(chunk, 0, e.Length);
            return chunk;
        }

        #endregion

        #region Receiving

        private void FileSendRequestReceivedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            switch (e.FileKind)
            {
                case ToxFileKind.Avatar:
                    ReceiveAvatar(e);
                    return;
            }
        }

        private void ReceiveAvatar(ToxEventArgs.FileSendRequestEventArgs e)
        {
            ToxErrorFileControl error;

            if (e.FileKind == ToxFileKind.Avatar && e.FileSize == 0) // It means the avatar of the friend is removed.
            {
                // So we cancel the transfer:
                ToxModel.Instance.FileControl(e.FriendNumber, e.FileNumber, ToxFileControl.Cancel, out error);
                // TODO: Error handling!
                // TODO: Actually remove avatar of the friend!
            }

            // TODO: Check the hash to see if we already have that avatar!

            ToxModel.Instance.FileControl(e.FriendNumber, e.FileNumber, ToxFileControl.Resume, out error);
            // TODO: Error handling!

            if (error == ToxErrorFileControl.Ok)
            {
                var stream = new MemoryStream((int) e.FileSize);
                _activeTransfers.Add(new TransferId(e.FileNumber, e.FriendNumber),
                    new TransferData(ToxFileKind.Avatar, stream, e.FileSize));
                Debug.WriteLine(
                    "File download added! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                    e.FriendNumber, e.FileNumber, _activeTransfers.Count);
            }
        }

        private void FileChunkReceivedHandler(object sender, ToxEventArgs.FileChunkEventArgs e)
        {
            var transferId = new TransferId(e.FileNumber, e.FriendNumber);
            if (IsTransferFinished(transferId))
                return;

            var currentTransfer = _activeTransfers[transferId];

            var currentStream = currentTransfer.Stream;
            PutNextChunk(e, currentStream);

            currentTransfer.IncreaseProgress(e.Data.Length);
            if (currentTransfer.IsFinished())
            {
                switch (currentTransfer.Kind)
                {
                    case ToxFileKind.Avatar:
                        AvatarManager.Instance.ChangeFriendAvatar(e.FriendNumber, currentStream as MemoryStream);
                        break;
                }

                _activeTransfers.Remove(transferId);
                Debug.WriteLine(
                    "File download removed! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                    e.FriendNumber, e.FileNumber, _activeTransfers.Count);
            }
        }

        private void PutNextChunk(ToxEventArgs.FileChunkEventArgs e, Stream currentStream)
        {
            if (currentStream.Position != e.Position)
                currentStream.Seek(e.Position, SeekOrigin.Begin);
            currentStream.Write(e.Data, 0, e.Data.Length);
        }

        #endregion

        #region Helper classes

        private class TransferId : IEquatable<TransferId>
        {
            public TransferId(int fileNumber, int friendNumber)
            {
                FileNumber = fileNumber;
                FriendNumber = friendNumber;
            }

            public int FileNumber { get; private set; }
            public int FriendNumber { get; private set; }

            public bool Equals(TransferId other)
            {
                return (FileNumber == other.FileNumber) && (FriendNumber == other.FriendNumber);
            }

            public override int GetHashCode()
            {
                return FriendNumber | (FileNumber << 1);
            }
        }

        private class TransferData
        {
            private readonly long _dataSizeInBytes;
            private long _transferredBytes;

            public TransferData(ToxFileKind kind, Stream stream, long dataSizeInBytes)
            {
                _transferredBytes = 0;
                _dataSizeInBytes = dataSizeInBytes;
                Kind = kind;
                Stream = stream;
            }

            public ToxFileKind Kind { get; private set; }
            public Stream Stream { get; private set; }

            public void IncreaseProgress(long amount)
            {
                _transferredBytes += amount;
            }

            public bool IsFinished()
            {
                return _transferredBytes == _dataSizeInBytes;
            }
        }

        #endregion
    }
}