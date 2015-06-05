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

            var transfer = FindTransferViewModel(fileNumber);
            if (transfer != null && transfer.IsNotPlaceholder)
            {
                switch (fileControl)
                {
                    case ToxFileControl.Cancel:
                        transfer.CancelTransferByFriend();
                        return;
                }
            }
        }

        private void ProgressChangedHandler(int friendNumber, int fileNumber, double newProgress)
        {
            if (friendNumber != _friendNumber)
                return;

            var transfer = FindTransferViewModel(fileNumber);
            if (transfer != null && transfer.IsNotPlaceholder)
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

        public void CancelTransfer(int fileNumber)
        {
            FileTransferManager.Instance.CancelTransfer(_friendNumber, fileNumber);
            var transfer = FindTransferViewModel(fileNumber);
            if (transfer != null)
                Transfers.Remove(transfer);
        }

        private OneFileTransferViewModel FindTransferViewModel(int fileNumber)
        {
            return Transfers.FirstOrDefault(transfer => transfer.FileNumber == fileNumber);
        }
    }
}