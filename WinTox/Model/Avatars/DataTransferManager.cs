using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SharpTox.Core;

namespace WinTox.Model.Avatars
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
        protected DataTransferManager()
        {
            Transfers = new Dictionary<TransferId, TransferData>();

            ToxModel.Instance.FileControlReceived += FileControlReceivedHandler;
            ToxModel.Instance.FileChunkRequested += FileChunkRequestedHandler;
            ToxModel.Instance.FileSendRequestReceived += FileSendRequestReceivedHandler;
            ToxModel.Instance.FileChunkReceived += FileChunkReceivedHandler;
        }

        #region Data model

        protected readonly Dictionary<TransferId, TransferData> Transfers;

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
            protected Stream Stream;

            public TransferData(Stream stream, long dataSizeInBytes, TransferDirection direction,
                long transferredBytes = 0)
            {
                Stream = stream;
                if (Stream != null)
                {
                    if (Stream.CanWrite)
                        Stream.SetLength(dataSizeInBytes);

                    Stream.Position = transferredBytes;
                }
                Direction = direction;
            }

            public TransferDirection Direction { get; private set; }

            public double Progress
            {
                get
                {
                    lock (Stream)
                    {
                        return ((double) Stream.Position/Stream.Length)*100;
                    }
                }
            }

            public long TransferredBytes
            {
                get { return Stream.Position; }
            }

            public bool IsFinished()
            {
                lock (Stream)
                {
                    return Stream.Position == Stream.Length;
                }
            }

            public byte[] GetNextChunk(ToxEventArgs.FileRequestChunkEventArgs e)
            {
                lock (Stream)
                {
                    if (Stream.Position != e.Position)
                    {
                        Stream.Seek(e.Position, SeekOrigin.Begin);
                    }

                    var chunk = new byte[e.Length];
                    Stream.Read(chunk, 0, e.Length);

                    return chunk;
                }
            }

            public void PutNextChunk(ToxEventArgs.FileChunkEventArgs e)
            {
                lock (Stream)
                {
                    if (Stream.Position != e.Position)
                    {
                        Stream.Seek(e.Position, SeekOrigin.Begin);
                    }

                    Stream.Write(e.Data, 0, e.Data.Length);
                }
            }

            public void Dispose()
            {
                if (Stream != null) // It could be a dummy transfer waiting for accept from the user!
                    Stream.Dispose();
            }
        }

        #endregion

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

        protected void AddTransfer(int friendNumber, int fileNumber, TransferData transferData)
        {
            Transfers.Add(new TransferId(friendNumber, fileNumber), transferData);
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
    }
}