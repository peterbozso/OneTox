using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SharpTox.Core;

namespace WinTox.Model
{
    public enum TransferDirection
    {
        Up,
        Down
    }

    /// <summary>
    ///     Abstract base class for TransferManager classes.
    /// </summary>
    public abstract class DataTransferManager
    {
        protected readonly Dictionary<TransferId, TransferData> Transfers;

        protected DataTransferManager()
        {
            Transfers = new Dictionary<TransferId, TransferData>();

            ToxModel.Instance.FileControlReceived += FileControlReceivedHandler;
            ToxModel.Instance.FileChunkRequested += FileChunkRequestedHandler;
            ToxModel.Instance.FileSendRequestReceived += FileSendRequestReceivedHandler;
            ToxModel.Instance.FileChunkReceived += FileChunkReceivedHandler;
        }

        #region Common

        private bool IsTransferFinished(TransferId transferId)
        {
            return !Transfers.ContainsKey(transferId);
        }

        private void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            Debug.WriteLine("File control received \t friend number: {0}, \t file number: {1}, \t control: {2}",
                e.FriendNumber, e.FileNumber, e.Control);

            var transferId = new TransferId(e.FriendNumber, e.FileNumber);

            if (Transfers.ContainsKey(transferId))
                HandleFileControl(e.Control, transferId);
        }

        protected abstract void HandleFileControl(ToxFileControl fileControl, TransferId transferId);

        protected void SendCancelControl(int friendNumber, int fileNumber)
        {
            ToxModel.Instance.FileControl(friendNumber, fileNumber, ToxFileControl.Cancel);
        }

        /// <summary>
        ///     Send a ToxFileControl.Resume to the selected friend for the given transfer.
        /// </summary>
        /// <param name="friendNumber">The friend's friend number.</param>
        /// <param name="fileNumber">The file number associated with the transfer.</param>
        /// <returns>True on success, false otherwise.</returns>
        protected bool SendResumeControl(int friendNumber, int fileNumber)
        {
            return ToxModel.Instance.FileControl(friendNumber, fileNumber, ToxFileControl.Resume);
        }

        protected void AddTransfer(int friendNumber, int fileNumber, Stream stream, long dataSizeInBytes,
            TransferDirection direction, long transferredBytes = 0)
        {
            Transfers.Add(new TransferId(friendNumber, fileNumber),
                new TransferData(stream, dataSizeInBytes, direction, transferredBytes));
        }

        protected void RemoveTransfer(TransferId transferId)
        {
            if (!Transfers.ContainsKey(transferId))
                return;

            var transferToRemove = Transfers[transferId];
            transferToRemove.Dispose();
            Transfers.Remove(transferId);
        }

        #endregion

        #region Sending

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            var transferId = new TransferId(e.FriendNumber, e.FileNumber);

            if (IsTransferFinished(transferId))
                return;

            var currentTransfer = Transfers[transferId];

            var chunk = currentTransfer.GetNextChunk(e);
            var successfulChunkSend = ToxModel.Instance.FileSendChunk(e.FriendNumber, e.FileNumber, e.Position, chunk);

            if (successfulChunkSend)
            {
                if (currentTransfer.IsFinished())
                {
                    HandleFinishedUpload(transferId, e.FriendNumber, e.FileNumber);
                }
            }
        }

        protected abstract void HandleFinishedUpload(TransferId transferId, int friendNumber, int fileNumber);

        #endregion

        #region Receiving

        protected abstract void FileSendRequestReceivedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e);

        private void FileChunkReceivedHandler(object sender, ToxEventArgs.FileChunkEventArgs e)
        {
            var transferId = new TransferId(e.FriendNumber, e.FileNumber);

            if (IsTransferFinished(transferId))
                return;

            var currentTransfer = Transfers[transferId];

            currentTransfer.PutNextChunk(e);

            if (currentTransfer.IsFinished())
            {
                HandleFinishedDownload(transferId, e.FriendNumber, e.FileNumber);
            }
        }

        protected abstract void HandleFinishedDownload(TransferId transferId, int friendNumber, int fileNumber);

        #endregion

        #region Helper classes

        protected class TransferId : IEquatable<TransferId>
        {
            public TransferId(int friendNumber, int fileNumber)
            {
                FriendNumber = friendNumber;
                FileNumber = fileNumber;
            }

            public int FriendNumber { get; private set; }
            public int FileNumber { get; private set; }

            public bool Equals(TransferId other)
            {
                return (FriendNumber == other.FriendNumber) && (FileNumber == other.FileNumber);
            }

            public override int GetHashCode()
            {
                return FriendNumber | (FileNumber << 1);
            }
        }

        protected class TransferData
        {
            private readonly long _dataSizeInBytes;

            public TransferData(Stream stream, long dataSizeInBytes, TransferDirection direction,
                long transferredBytes = 0)
            {
                _dataSizeInBytes = dataSizeInBytes;

                _stream = stream;
                if (_stream != null)
                {
                    if (_stream.CanWrite)
                        _stream.SetLength(_dataSizeInBytes);

                    _stream.Position = transferredBytes;
                }
                Direction = direction;
            }

            public TransferDirection Direction { get; private set; }
            private Stream _stream;

            public double Progress
            {
                get
                {
                    lock (_stream)
                    {
                        return ((double) _stream.Position/_stream.Length)*100;
                    }
                }
            }

            public long TransferredBytes { get { return _stream.Position; } }

            public bool IsFinished()
            {
                lock (_stream)
                {
                    return _stream.Position == _stream.Length;
                }
            }

            public void ReplaceStream(Stream newStream)
            {
                if (_stream != null) // We only allow replacement of a dummy stream.
                    return;

                newStream.SetLength(_dataSizeInBytes);
                _stream = newStream;
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

            public void Dispose()
            {
                if (_stream != null) // It could be a dummy transfer waiting for accept from the user!
                    _stream.Dispose(); 
            }
        }

        #endregion
    }
}