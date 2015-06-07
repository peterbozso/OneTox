using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel.FileTransfer
{
    public class FileTransfersViewModel
    {
        private readonly int _friendNumber;

        public FileTransfersViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            Transfers = new ObservableCollection<OneFileTransferViewModel>();
            FileTransferManager.Instance.ProgressChanged += ProgressChangedHandler;
            FileTransferManager.Instance.FileControlReceived += FileControlReceivedHandler;
            FileTransferManager.Instance.FileSendRequestReceived += FileSendRequestReceivedHandler;
        }

        public ObservableCollection<OneFileTransferViewModel> Transfers { get; private set; }

        #region Helper search methods

        private OneFileTransferViewModel FindNotPlaceHolderTransferViewModel(int fileNumber)
        {
            return Transfers.FirstOrDefault(transfer => transfer.FileNumber == fileNumber && transfer.IsNotPlaceholder);
            // There can be multiple transfers with the same file number, but there's always only one that's not a placeholder.
        }

        #endregion

        #region Actions coming from the View

        public async Task SendFile(StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            int fileNumber;
            var successfulSend = FileTransferManager.Instance.SendFile(_friendNumber, stream, file.Name, out fileNumber);
            if (successfulSend)
            {
                Transfers.Add(new OneFileTransferViewModel(this, fileNumber, file.Name, FileTransferState.Uploading));
            }
        }

        public void AcceptTransferByUser(int fileNumber, Stream saveStream)
        {
            FileTransferManager.Instance.ReceiveFile(_friendNumber, fileNumber, saveStream);
        }

        public void CancelTransferByUser(OneFileTransferViewModel transferViewModel)
        {
            FileTransferManager.Instance.CancelTransfer(_friendNumber, transferViewModel.FileNumber);
            Transfers.Remove(transferViewModel);
        }

        public void PauseTransferByUser(int fileNumber)
        {
            FileTransferManager.Instance.PauseTransfer(_friendNumber, fileNumber);
        }

        public void ResumeTransferByUser(int fileNumber)
        {
            FileTransferManager.Instance.ResumeTransfer(_friendNumber, fileNumber);
        }

        #endregion

        #region Actions coming from the Model

        private void FileControlReceivedHandler(int friendNumber, int fileNumber, ToxFileControl fileControl)
        {
            if (friendNumber != _friendNumber)
                return;

            var transfer = FindNotPlaceHolderTransferViewModel(fileNumber);
            if (transfer == null)
                return;

            switch (fileControl)
            {
                case ToxFileControl.Cancel:
                    transfer.CancelTransferByFriend();
                    return;
                case ToxFileControl.Pause:
                    transfer.PauseTransferByFriend();
                    return;
                case ToxFileControl.Resume:
                    transfer.ResumeTransferByFriend();
                    return;
            }
        }

        private void ProgressChangedHandler(int friendNumber, int fileNumber, double newProgress)
        {
            if (friendNumber != _friendNumber)
                return;

            var transfer = FindNotPlaceHolderTransferViewModel(fileNumber);
            if (transfer == null)
                return;

            if (newProgress.Equals(100.0))
                transfer.FinishTransfer();
            else
                transfer.Progress = newProgress;
        }

        private void FileSendRequestReceivedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FriendNumber != _friendNumber)
                return;

            Transfers.Add(new OneFileTransferViewModel(this, e.FileNumber, e.FileName, FileTransferState.Downloading));
        }

        #endregion
    }
}