using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using WinTox.Model;

namespace WinTox.ViewModel.FileTransfer
{
    public class FileTransfersViewModel
    {
        private readonly int _friendNumber;

        public FileTransfersViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            Transfers = new ObservableCollection<FileTransferDataViewModel>();
            FileTransferManager.Instance.ProgressChanged += ProgressChangedHandler;
        }

        public ObservableCollection<FileTransferDataViewModel> Transfers { get; private set; }

        private void ProgressChangedHandler(int fileNumber, double newProgress)
        {
            var transfer = FindTransferViewModel(fileNumber);
            if (transfer != null)
                transfer.Progress = newProgress;
        }

        public async Task SendFile(StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            int fileNumber;
            var successfulSend = FileTransferManager.Instance.SendFile(_friendNumber, stream, file.Name, out fileNumber);
            if (successfulSend)
            {
                Transfers.Add(new FileTransferDataViewModel(this, fileNumber, file.Name, FileTransferDirection.Up));
            }
        }

        public void CancelTransfer(int fileNumber)
        {
            FileTransferManager.Instance.CancelTransfer(_friendNumber, fileNumber);
            var transfer = FindTransferViewModel(fileNumber);
            Transfers.Remove(transfer);
        }

        private FileTransferDataViewModel FindTransferViewModel(int fileNumber)
        {
            return Transfers.FirstOrDefault(transfer => transfer.FileNumber == fileNumber);
        }
    }
}