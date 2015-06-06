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
        }

        public ObservableCollection<OneFileTransferViewModel> Transfers { get; private set; }

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

        public void CancelTransferByUser(int fileNumber)
        {
            FileTransferManager.Instance.CancelTransfer(_friendNumber, fileNumber);
            var transfer = FindTransferViewModel(fileNumber);
            if (transfer != null)
                Transfers.Remove(transfer);
        }

        public void PauseTransferByUser(int fileNumber)
        {
            FileTransferManager.Instance.PauseTransfer(_friendNumber, fileNumber);
        }

        public void ResumeTransferByUser(int fileNumber)
        {
            FileTransferManager.Instance.ResumeTransfer(_friendNumber, fileNumber);
        }

        private OneFileTransferViewModel FindTransferViewModel(int fileNumber)
        {
            return Transfers.FirstOrDefault(transfer => transfer.FileNumber == fileNumber);
        }

        private OneFileTransferViewModel FindNotPlaceHolderTransferViewModel(int fileNumber)
        {
            return Transfers.FirstOrDefault(transfer => transfer.FileNumber == fileNumber && transfer.IsNotPlaceholder);
            // There can be multiple transfers with the same file number, but there's always only one that's not a placeholder.
        }
    }
}