using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using SharpTox.Core;
using WinTox.Model.Avatars;

namespace WinTox.Model.FileTransfers
{
    public class FileTransfersModel
    {
        private readonly int _friendNumber;

        public FileTransfersModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            ToxModel.Instance.FileSendRequestReceived += FileSendRequestReceivedHandler;
        }

        private async void FileSendRequestReceivedHandler(object sender,
            ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FileKind != ToxFileKind.Data || e.FriendNumber != _friendNumber)
                return;

            var fileId = ToxModel.Instance.FileGetId(e.FriendNumber, e.FileNumber);
            var resumeData = await FileTransferResumer.Instance.GetDownloadData(fileId);

            OneFileTransferModel oneFileTransferModel;

            // If we could find the resume data for this download, we resume it instead of handling it as a new transfer.
            if (resumeData != null)
            {
                oneFileTransferModel =
                    await
                        OneBrokenFileDownloadModel.CreateInstance(e.FriendNumber, e.FileNumber, resumeData.File.Name,
                            e.FileSize, TransferDirection.Down, resumeData.File, resumeData.TransferredBytes);
            }
            else
            {
                // We add a transfer with a null value instead of an actual stream here. We will replace it with an actual file stream
                // in OneFileTransferModel.AcceptTransfer() when the user accepts the request and chooses a location to save the file to.
                oneFileTransferModel =
                    await
                        OneFileTransferModel.CreateInstance(e.FriendNumber, e.FileNumber, e.FileName, e.FileSize,
                            TransferDirection.Down, null);
            }

            if (FileSendRequestReceived != null)
                FileSendRequestReceived(this, oneFileTransferModel);
        }

        public event EventHandler<OneFileTransferModel> FileSendRequestReceived;

        public async Task<OneFileTransferModel> SendFile(StorageFile file)
        {
            var fileProperties = await file.GetBasicPropertiesAsync();
            var fileSizeInBytes = (long) fileProperties.Size;

            bool successfulFileSend;
            var fileInfo = ToxModel.Instance.FileSend(_friendNumber, ToxFileKind.Data, fileSizeInBytes, file.Name,
                out successfulFileSend);

            if (successfulFileSend)
            {
                // FileTransferResumer.Instance.RecordTransfer(file, _friendNumber, fileNumber, TransferDirection.Up);
                Debug.WriteLine("STUB: SendFile()!");

                var transferModel = await OneFileTransferModel.CreateInstance(_friendNumber, fileInfo.Number, file.Name,
                    fileSizeInBytes, TransferDirection.Up, file);
                return transferModel;
            }

            return null;
        }
    }
}