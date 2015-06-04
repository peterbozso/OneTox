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
        public delegate void ProgressChangedDelegate(int fileNumber, double newProgress);

        private static FileTransferManager _instance;

        private FileTransferManager()
        {
        }

        public static FileTransferManager Instance
        {
            get { return _instance ?? (_instance = new FileTransferManager()); }
        }

        protected override async void InCreaseTransferProgress(TransferData transferData, int amount, int fileNumber)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                base.InCreaseTransferProgress(transferData, amount, fileNumber);
                if (ProgressChanged != null)
                    ProgressChanged(fileNumber, transferData.GetProgress());
            });
        }

        public event ProgressChangedDelegate ProgressChanged;

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

        #endregion

        #region Receiving

        protected override void FileSendRequestReceivedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
        }

        protected override void HandleFinishedDownload(TransferId transferId, ToxEventArgs.FileChunkEventArgs e)
        {
        }

        #endregion
    }
}