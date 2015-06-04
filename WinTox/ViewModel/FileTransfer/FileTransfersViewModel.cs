using System;
using System.Collections.ObjectModel;
using System.IO;
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
            foreach (var transfer in Transfers)
            {
                if (transfer.FileNumber == fileNumber)
                {
                    transfer.Progress = newProgress;
                    return;
                }
            }
        }

        public async Task SendFile(StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            int fileNumber;
            var successfulSend = FileTransferManager.Instance.SendFile(_friendNumber, stream, file.Name, out fileNumber);
            if (successfulSend)
            {
                Transfers.Add(new FileTransferDataViewModel(fileNumber, file.DisplayName, FileTransferDirection.Up));
            }
        }
    }
}