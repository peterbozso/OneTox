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
            Transfers.Add(new TransferId(friendNumber, fileNumber), new TransferData(stream, dataSizeInBytes, direction, transferredBytes));
        }

        protected void RemoveTransfer(TransferId transferId)
        {
            var transferToRemove = Transfers[transferId];
            if (transferToRemove.Stream != null) // It could be a dummy transfer waiting for accept from the user!
                transferToRemove.Stream.Dispose();
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

            var chunk = GetNextChunk(e, currentTransfer);
            bool successfulChunkSend;
            ToxModel.Instance.FileSendChunk(e.FriendNumber, e.FileNumber, e.Position, chunk, out successfulChunkSend);

            if (successfulChunkSend)
            {
                currentTransfer.IncreaseProgress(e.Length);
                if (currentTransfer.IsFinished())
                {
                    HandleFinishedUpload(transferId, e.FriendNumber, e.FileNumber);
                }
            }
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
            var currentStream = currentTransfer.Stream;

            PutNextChunk(e, currentStream);

            currentTransfer.IncreaseProgress(e.Data.Length);
            if (currentTransfer.IsFinished())
            {
                HandleFinishedDownload(transferId, e.FriendNumber, e.FileNumber);
            }
        }

        private void PutNextChunk(ToxEventArgs.FileChunkEventArgs e, Stream currentStream)
        {
            if (currentStream.Position != e.Position)
                currentStream.Seek(e.Position, SeekOrigin.Begin);

            currentStream.Write(e.Data, 0, e.Data.Length);
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

            public TransferData(Stream stream, long dataSizeInBytes, TransferDirection direction, long transferredBytes = 0)
            {
                TransferredBytes = transferredBytes;
                _dataSizeInBytes = dataSizeInBytes;
                Stream = stream;
                Direction = direction;
            }

            public TransferDirection Direction { get; private set; }
            public Stream Stream { get; private set; }

            public double Progress
            {
                get { return ((double)TransferredBytes / _dataSizeInBytes) * 100; }
            }

            public void IncreaseProgress(long amount)
            {
                TransferredBytes += amount;
            }

            public long TransferredBytes { get; private set; }

            public bool IsFinished()
            {
                return TransferredBytes == _dataSizeInBytes;
            }

            public void ReplaceStream(Stream newStream)
            {
                newStream.SetLength(_dataSizeInBytes);
                Stream = newStream;
            }
        }

        #endregion
    }
}