﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SharpTox.Core;

namespace WinTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    internal class FileTransferManager : DataTransferManager
    {
        private static FileTransferManager _instance;

        private FileTransferManager()
        {
        }

        public static FileTransferManager Instance
        {
            get { return _instance ?? (_instance = new FileTransferManager()); }
        }

        #region Debug

        protected new void AddTransfer(int friendNumber, int fileNumber, Stream stream, long dataSizeInBytes,
            TransferDirection direction)
        {
            base.AddTransfer(friendNumber, fileNumber, stream, dataSizeInBytes, direction);

            Debug.WriteLine(
                "File {0}load added! \t friend number: {1}, \t file number: {2}, \t total avatar transfers: {3}",
                direction, friendNumber, fileNumber, Transfers.Count);
        }

        protected new void RemoveTransfer(TransferId transferId)
        {
            var direction = Transfers[transferId].Direction;

            base.RemoveTransfer(transferId);

            Debug.WriteLine(
                "File {0}load removed! \t friend number: {1}, \t file number: {2}, \t total avatar transfers: {3}",
                direction, transferId.FriendNumber, transferId.FileNumber, Transfers.Count);
        }

        #endregion


        #region Common

        public void CancelTransfer(int friendNumber, int fileNumber)
        {
            SendCancelControl(friendNumber, fileNumber);

            var transferId = new TransferId(fileNumber, friendNumber);
            if (Transfers.ContainsKey(transferId))
            {
                RemoveTransfer(transferId);
            }
        }

        public void PauseTransfer(int friendNumber, int fileNumber)
        {
            ToxModel.Instance.FileControl(friendNumber, fileNumber, ToxFileControl.Pause);
        }

        public void ResumeTransfer(int friendNumber, int fileNumber)
        {
            SendResumeControl(friendNumber, fileNumber);
        }

        protected override void HandleFileControl(ToxFileControl fileControl, TransferId transferId)
        {
            if (FileControlReceived != null)
                FileControlReceived(transferId.FriendNumber, transferId.FileNumber, fileControl);

            switch (fileControl)
            {
                case ToxFileControl.Cancel:
                    RemoveTransfer(transferId);
                    return;
            }
        }

        public Dictionary<int, double> GetTransferProgressesOfFriend(int friendNumber)
        {
            var progressDict = new Dictionary<int, double>();
            foreach (var transfer in Transfers)
            {
                if (transfer.Key.FriendNumber == friendNumber)
                    progressDict.Add(transfer.Key.FileNumber, transfer.Value.Progress);
            }
            return progressDict;
        }

        private void RaiseTransferFinished(int friendNumber, int fileNumber)
        {
            if (TransferFinished != null)
                TransferFinished(friendNumber, fileNumber);
        }

        public delegate void FileControlReceivedDelegate(int friendNumber, int fileNumber, ToxFileControl fileControl);

        public event FileControlReceivedDelegate FileControlReceived;

        public delegate void TransferfinishedDelegate(int friendNumber, int fileNumber);

        public event TransferfinishedDelegate TransferFinished;

        #endregion

        #region Sending

        public bool SendFile(int friendNumber, Stream stream, string fileName, out int fileNumber)
        {
            bool successfulFileSend;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Data, stream.Length, fileName,
                out successfulFileSend);

            if (successfulFileSend)
            {
                AddTransfer(friendNumber, fileInfo.Number, stream, stream.Length, TransferDirection.Up);
            }

            fileNumber = fileInfo.Number;
            return successfulFileSend;
        }

        protected override void HandleFinishedUpload(TransferId transferId, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            RemoveTransfer(transferId);
            RaiseTransferFinished(e.FriendNumber, e.FileNumber);
        }

        #endregion

        #region Receiving

        public event EventHandler<ToxEventArgs.FileSendRequestEventArgs> FileSendRequestReceived;

        public void ReceiveFile(int friendNumber, int fileNumber, Stream saveStream)
        {
            var transferId = new TransferId(fileNumber, friendNumber);

            // Replace the dummy stream set it FileSendRequestReceivedHandler():
            Transfers[transferId].ReplaceStream(saveStream);

            SendResumeControl(friendNumber, fileNumber);
        }

        protected override void FileSendRequestReceivedHandler(object sender,
            ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FileKind != ToxFileKind.Data)
                return;

            // We add a transfer with a null value instead of an actual stream here. We will replace it with an actual file stream
            // in ReceiveFile() when the user accepts the request and chooses a file.
            AddTransfer(e.FriendNumber, e.FileNumber, null, e.FileSize, TransferDirection.Down);

            if (FileSendRequestReceived != null)
                FileSendRequestReceived(this, e);
        }

        protected override void HandleFinishedDownload(TransferId transferId, ToxEventArgs.FileChunkEventArgs e)
        {
            RemoveTransfer(transferId);
            RaiseTransferFinished(e.FriendNumber, e.FileNumber);
        }

        #endregion
    }
}