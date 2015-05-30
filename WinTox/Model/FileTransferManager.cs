using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private class TransferId : IEquatable<TransferId>
        {
            public TransferId(int fileNumber, int friendNumber)
            {
                FileNumber = fileNumber;
                FriendNumber = friendNumber;
            }

            public int FileNumber { get; private set; }
            public int FriendNumber { get; private set; }

            public override int GetHashCode()
            {
                var fileId = ToxModel.Instance.FileGetId(FriendNumber, FileNumber);
                return fileId.Aggregate(0, (current, val) => current + val);
            }

            public bool Equals(TransferId other)
            {
                return (this.FileNumber == other.FileNumber) && (this.FriendNumber == other.FriendNumber);
            }
        }

        private class TransferData
        {
            public ToxFileKind Kind { get; set; }
            public Stream Stream { get; set; }
        }

        private static FileTransferManager _instance;
        private readonly Dictionary<TransferId, TransferData> _activeTransfers;

        private FileTransferManager()
        {
            _activeTransfers = new Dictionary<TransferId, TransferData>();
            ToxModel.Instance.FileControlReceived += FileControlReceivedHandler;
            ToxModel.Instance.FileChunkRequested += FileChunkRequestedHandler;
            ToxModel.Instance.FileSendRequestReceived += FileSendRequestReceivedHandler;
            ToxModel.Instance.FileChunkReceived += FileChunkReceivedHandler;
        }

        public static FileTransferManager Instance
        {
            get { return _instance ?? (_instance = new FileTransferManager()); }
        }

        private void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            if (e.Control == ToxFileControl.Cancel)
            {
                _activeTransfers.Remove(new TransferId(e.FileNumber, e.FriendNumber));
            }
            // TODO: Add handling for other types of Control!
        }

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            var transferId = new TransferId(e.FileNumber, e.FriendNumber);
            if (e.Length == 0) // File transfer is complete
            {
                _activeTransfers.Remove(transferId);
                return;
            }

            var currentTransferStream = _activeTransfers[transferId].Stream;
            lock (currentTransferStream)
            {
                if (e.Position != currentTransferStream.Position)
                    currentTransferStream.Seek(e.Position, SeekOrigin.Begin);
            }
            var chunk = new byte[e.Length];
            currentTransferStream.Read(chunk, 0, e.Length);
            ToxErrorFileSendChunk error;
            ToxModel.Instance.FileSendChunk(e.FriendNumber, e.FileNumber, e.Position, chunk, out error);
            // TODO: Error handling!
        }

        public async Task SendAvatar(int friendNumber, StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            ToxErrorFileSend error;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, stream.Length, file.Name,
                GetAvatarHash(stream), out error);
            if (error == ToxErrorFileSend.Ok)
            {
                _activeTransfers.Add(new TransferId(fileInfo.Number, friendNumber), new TransferData{ Kind = ToxFileKind.Avatar, Stream = stream });
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

        private void FileSendRequestReceivedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            switch (e.FileKind)
            {
                case ToxFileKind.Avatar:
                    HandleAvatarReception(e);
                    return;
            }
        }

        private void HandleAvatarReception(ToxEventArgs.FileSendRequestEventArgs e)
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
            var stream = new MemoryStream((int) e.FileSize);
            _activeTransfers.Add(new TransferId(e.FileNumber, e.FriendNumber), new TransferData{ Kind = e.FileKind, Stream = stream });
        }

        private void FileChunkReceivedHandler(object sender, ToxEventArgs.FileChunkEventArgs e)
        {
            var transferId = new TransferId(e.FileNumber, e.FriendNumber);
            var currentTransfer = _activeTransfers[transferId];
            var currentStream = currentTransfer.Stream;

            if (e.Data == null) // The transfer is finished.
            {
                switch (currentTransfer.Kind)
                {
                    case ToxFileKind.Avatar:
                        AvatarManager.Instance.SetFriendAvatar(e.FriendNumber, currentStream as MemoryStream);
                        break;
                }
                _activeTransfers.Remove(transferId);
                return;
            }

            if (currentStream.Position != e.Position)
                currentStream.Seek(e.Position, SeekOrigin.Begin);
            currentStream.Write(e.Data, 0, e.Data.Length);
        }
    }
}