using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;

namespace WinTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    internal class FileTransferManager : DataTransferManager
    {
        public delegate void FileControlReceivedDelegate(int friendNumber, int fileNumber, ToxFileControl fileControl);

        public delegate void ProgressChangedDelegate(int friendNumber, int fileNumber, double newProgress);

        private static FileTransferManager _instance;

        private FileTransferManager()
        {
        }

        public static FileTransferManager Instance
        {
            get { return _instance ?? (_instance = new FileTransferManager()); }
        }

        protected override async void InCreaseTransferProgress(TransferData transferData, int amount, int fileNumber,
            int friendNumber)
        {
            base.InCreaseTransferProgress(transferData, amount, fileNumber, friendNumber);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (ProgressChanged != null)
                    ProgressChanged(friendNumber, fileNumber, transferData.GetProgress());
            });
        }

        public void CancelTransfer(int friendNumber, int fileNumber)
        {
            SendCancelControl(friendNumber, fileNumber);

            var transferId = new TransferId(fileNumber, friendNumber);
            if (ActiveTransfers.ContainsKey(transferId))
            {
                ActiveTransfers.Remove(transferId);
                Debug.WriteLine(
                    "File transfer CANCELLED (removed) by user! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                    friendNumber, fileNumber, ActiveTransfers.Count);
            }
        }

        public event ProgressChangedDelegate ProgressChanged;

        protected override async void HandleFileControl(ToxFileControl fileControl, TransferId transferId)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (FileControlReceived != null)
                    FileControlReceived(transferId.FriendNumber, transferId.FileNumber, fileControl);
            });

            switch (fileControl)
            {
                case ToxFileControl.Cancel:
                    ActiveTransfers.Remove(transferId);
                    Debug.WriteLine(
                        "File transfer CANCELLED by friend! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                        transferId.FriendNumber, transferId.FileNumber, ActiveTransfers.Count);
                    return;
            }
        }

        public event FileControlReceivedDelegate FileControlReceived;

        #region Sending

        public bool SendFile(int friendNumber, Stream stream, string fileName, out int fileNumber)
        {
            bool successfulFileSend;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Data, stream.Length, fileName,
                new byte[ToxConstants.FileIdLength], out successfulFileSend);

            if (successfulFileSend)
            {
                ActiveTransfers.Add(new TransferId(fileInfo.Number, friendNumber),
                    new TransferData(ToxFileKind.Avatar, stream, stream.Length));
                Debug.WriteLine(
                    "File upload added! \t friend number: {0}, \t file number: {1}, \t total file transfers: {2}",
                    friendNumber, fileInfo.Number, ActiveTransfers.Count);
            }

            fileNumber = fileInfo.Number;
            return successfulFileSend;
        }

        protected override void HandleFinishedUpload(TransferId transferId, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            ActiveTransfers.Remove(transferId);

            Debug.WriteLine(
                "File upload removed! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                e.FriendNumber, e.FileNumber, ActiveTransfers.Count);
        }

        #endregion

        #region Receiving

        protected override void FileSendRequestReceivedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            Debug.WriteLine("STUB: FileTransferManager.FileSendRequestReceivedHandler()");
        }

        protected override void HandleFinishedDownload(TransferId transferId, ToxEventArgs.FileChunkEventArgs e)
        {
            Debug.WriteLine("STUB: FileTransferManager.HandleFinishedDownload()");
        }

        #endregion
    }
}