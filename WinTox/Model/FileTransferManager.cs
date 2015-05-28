using System;
using System.Collections.Generic;
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
        private static FileTransferManager _instance;
        private readonly Dictionary<ToxFileInfo, Stream> _activeTransfers;

        private FileTransferManager()
        {
            _activeTransfers = new Dictionary<ToxFileInfo, Stream>();
            ToxModel.Instance.FileControlReceived += FileControlReceivedHandler;
            ToxModel.Instance.FileChunkRequested += FileChunkRequestedHandler;
        }

        public static FileTransferManager Instance
        {
            get { return _instance ?? (_instance = new FileTransferManager()); }
        }

        private void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            if (e.Control == ToxFileControl.Cancel)
            {
                RemoveActiveTransfer(e.FileNumber);
            }
            // TODO: Add handling for other types of Control!
        }

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            if (e.Length == 0) // File transfer is complete
            {
                RemoveActiveTransfer(e.FileNumber);
                return;
            }

            var currentTransferStream = _activeTransfers[FindFileInfo(e.FileNumber)];
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

        private void RemoveActiveTransfer(int fileNumber)
        {
            _activeTransfers.Remove(FindFileInfo(fileNumber));
        }

        private ToxFileInfo FindFileInfo(int fileNumber)
        {
            foreach (var transfer in _activeTransfers.Where(transfer => transfer.Key.Number == fileNumber))
            {
                return transfer.Key;
            }
            throw new ArgumentException();
        }

        public async Task Send(int friendNumber, ToxFileKind kind, StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            ToxErrorFileSend error;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, kind, stream.Length, file.Name,
                GenerateFileHash(stream), out error);
            if (error == ToxErrorFileSend.Ok)
            {
                _activeTransfers.Add(fileInfo, stream);
            }
        }

        private byte[] GenerateFileHash(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            return ToxTools.Hash(buffer);
        }
    }
}