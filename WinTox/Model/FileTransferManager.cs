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
        private readonly Dictionary<int, Tuple<Stream, ToxFileKind>> _activeTransfers;

        private FileTransferManager()
        {
            _activeTransfers = new Dictionary<int, Tuple<Stream, ToxFileKind>>();
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
                _activeTransfers.Remove(e.FileNumber);
            }
            // TODO: Add handling for other types of Control!
        }

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            if (e.Length == 0) // File transfer is complete
            {
                _activeTransfers.Remove(e.FileNumber);
                return;
            }

            var currentTransferStream = _activeTransfers[e.FileNumber].Item1;
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
                _activeTransfers.Add(fileInfo.Number, new Tuple<Stream, ToxFileKind>(stream, ToxFileKind.Avatar));
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
            Debug.WriteLine("Reception starts : name: {0}, size: {1}, number: {2}", e.FileName, e.FileSize, e.FileNumber);
            ToxErrorFileControl error;
            if (e.FileKind == ToxFileKind.Avatar && e.FileSize == 0) // It means the avatar of the friend is removed.
            {
                // So we cancel the transfer:
                ToxModel.Instance.FileControl(e.FriendNumber, e.FileNumber, ToxFileControl.Cancel, out error);
                Debug.WriteLine("Reception cancelled due to existing avatar : name: {0}, size: {1}, number: {2}",
                    e.FileName, e.FileSize, e.FileNumber);
                // TODO: Error handling!
                // TODO: Actually remove avatar of the friend!
            }

            // TODO: Check the hash to see if we already have that avatar!

            ToxModel.Instance.FileControl(e.FriendNumber, e.FileNumber, ToxFileControl.Resume, out error);
            // TODO: Error handling!
            var stream = new MemoryStream {Capacity = (int) e.FileSize};
            _activeTransfers.Add(e.FileNumber, new Tuple<Stream, ToxFileKind>(stream, e.FileKind));
        }

        private void FileChunkReceivedHandler(object sender, ToxEventArgs.FileChunkEventArgs e)
        {
            var currentTransfer = _activeTransfers[e.FileNumber];
            var currentStream = currentTransfer.Item1;

            if (e.Data == null) // The transfer is finished.
            {
                switch (currentTransfer.Item2)
                {
                    case ToxFileKind.Avatar:
                        AvatarManager.Instance.ReceiveFriendAvatar(currentStream);
                        break;
                }
                _activeTransfers.Remove(e.FileNumber);
                return;
            }

            if (currentStream.Position != e.Position)
                currentStream.Seek(e.Position, SeekOrigin.Begin);
            currentStream.Write(e.Data, 0, e.Data.Length);
        }
    }
}