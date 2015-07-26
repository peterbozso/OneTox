using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpTox.Core;

namespace WinTox.Model
{
    public class FileTransfersModel
    {
        private readonly int _friendNumber;
        public ObservableCollection<OneFileTransferModel> Transfers;

        public FileTransfersModel(int friendNumber)
        {
            _friendNumber = friendNumber;

            Transfers = new ObservableCollection<OneFileTransferModel>();

            ToxModel.Instance.FileSendRequestReceived += FileSendRequestReceivedHandler;
        }

        private async void FileSendRequestReceivedHandler(object sender,
            ToxEventArgs.FileSendRequestEventArgs e)
        {
            Debug.WriteLine("STUB: FileSendRequestReceivedHandler()!");

            /*
            if (e.FileKind != ToxFileKind.Data)
                return;

            var fileId = ToxModel.Instance.FileGetId(e.FriendNumber, e.FileNumber);
            var resumeData = await FileTransferResumer.Instance.GetDownloadData(fileId);
            if (resumeData != null)
            {
                ResumeBrokenDownload(e, resumeData);
            }
            else
            {
                // We add a transfer with a null value instead of an actual stream here. We will replace it with an actual file stream
                // in ReceiveFile() when the user accepts the request and chooses a location to save the file to.
                Transfers.Add(new OneFileTransferModel(e.FriendNumber, e.FileNumber, e.FileName, e.FileSize,
                    TransferDirection.Down, null));
            }
            */
        }

        public async Task<OneFileTransferModel> SendFile(StorageFile file)
        {
            var fileStream = (await file.OpenReadAsync()).AsStreamForRead();

            bool successfulFileSend;
            var fileInfo = ToxModel.Instance.FileSend(_friendNumber, ToxFileKind.Data, fileStream.Length, file.Name,
                out successfulFileSend);

            if (successfulFileSend)
            {
                // FileTransferResumer.Instance.RecordTransfer(file, _friendNumber, fileNumber, TransferDirection.Up);
                Debug.WriteLine("STUB: SendFile()!");

                var transferModel = new OneFileTransferModel(this, _friendNumber, fileInfo.Number, file.Name,
                    fileStream.Length, TransferDirection.Up, fileStream);
                Transfers.Add(transferModel);
                return transferModel;
            }

            fileStream.Dispose();
            return null;
        }
    }
}