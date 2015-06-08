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
        private static FileTransferManager _instance;
        private readonly CoreDispatcher _dispatcher;

        private FileTransferManager()
        {
            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        }

        public static FileTransferManager Instance
        {
            get { return _instance ?? (_instance = new FileTransferManager()); }
        }

        #region Common

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

        public void PauseTransfer(int friendNumber, int fileNumber)
        {
            ToxModel.Instance.FileControl(friendNumber, fileNumber, ToxFileControl.Pause);
        }

        public void ResumeTransfer(int friendNumber, int fileNumber)
        {
            SendResumeControl(friendNumber, fileNumber);
        }

        protected override async void InCreaseTransferProgress(TransferId transferId, TransferData transferData,
            int amount)
        {
            base.InCreaseTransferProgress(transferId, transferData, amount);
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (ProgressChanged != null)
                    ProgressChanged(transferId.FriendNumber, transferId.FileNumber, transferData.GetProgress());
            });
        }


        protected override async void HandleFileControl(ToxFileControl fileControl, TransferId transferId)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

        #endregion

        #region Events

        public delegate void FileControlReceivedDelegate(int friendNumber, int fileNumber, ToxFileControl fileControl);

        public delegate void ProgressChangedDelegate(int friendNumber, int fileNumber, double newProgress);

        public event ProgressChangedDelegate ProgressChanged;
        public event FileControlReceivedDelegate FileControlReceived;

        #endregion

        #region Sending

        public bool SendFile(int friendNumber, Stream stream, string fileName, out int fileNumber)
        {
            bool successfulFileSend;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Data, stream.Length, fileName,
                new byte[ToxConstants.FileIdLength], out successfulFileSend);

            if (successfulFileSend)
            {
                ActiveTransfers.Add(new TransferId(fileInfo.Number, friendNumber),
                    new TransferData(stream, stream.Length));
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

        public event EventHandler<ToxEventArgs.FileSendRequestEventArgs> FileSendRequestReceived;

        public void ReceiveFile(int friendNumber, int fileNumber, Stream saveStream)
        {
            var transferId = new TransferId(fileNumber, friendNumber);

            // Replace the dummy stream set it FileSendRequestReceivedHandler():
            ActiveTransfers[transferId].ReplaceStream(saveStream);

            SendResumeControl(friendNumber, fileNumber);
            Debug.WriteLine(
                "File download confirmed by user! \t friend number: {0}, \t file number: {1}, \t total avatar transfers: {2}",
                friendNumber, fileNumber, ActiveTransfers.Count);
        }

        protected override async void FileSendRequestReceivedHandler(object sender,
            ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FileKind != ToxFileKind.Data)
                return;

            // We add a transfer with a dummy stream here what we will change to an actual file stream in ReceiveFile()
            // when the user accepts the request and choose a file.
            ActiveTransfers.Add(new TransferId(e.FileNumber, e.FriendNumber),
                new TransferData(new MemoryStream(), e.FileSize));

            Debug.WriteLine(
                "Dummy file download added! \t friend number: {0}, \t file number: {1}, \t total file transfers: {2}",
                e.FriendNumber, e.FileNumber, ActiveTransfers.Count);

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (FileSendRequestReceived != null)
                    FileSendRequestReceived(this, e);
            });
        }

        protected override void HandleFinishedDownload(TransferId transferId, ToxEventArgs.FileChunkEventArgs e)
        {
            ActiveTransfers.Remove(transferId);

            Debug.WriteLine(
                "File download removed! \t friend number: {0}, \t file number: {1}, \t total file transfers: {2}",
                e.FriendNumber, e.FileNumber, ActiveTransfers.Count);
        }

        #endregion
    }
}