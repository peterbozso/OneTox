using System;
using System.Threading.Tasks;
using Windows.Storage;
using OneTox.Config;
using SharpTox.Core;

namespace OneTox.Model.FileTransfers
{
    public class FileTransfersModel
    {
        private readonly IDataService _dataService;
        private readonly IFileTransferResumer _fileTransferResumer;
        private readonly int _friendNumber;
        private readonly IToxModel _toxModel;

        public FileTransfersModel(IDataService dataService, int friendNumber)
        {
            _dataService = dataService;
            _toxModel = dataService.ToxModel;
            _fileTransferResumer = dataService.FileTransferResumer;

            _friendNumber = friendNumber;
            _toxModel.FileSendRequestReceived += FileSendRequestReceivedHandler;
            _toxModel.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
        }

        public event EventHandler<OneFileTransferModel> FileTransferAdded;

        private async Task<long> GetFileSizeInBytes(StorageFile file)
        {
            var fileProperties = await file.GetBasicPropertiesAsync();
            return (long) fileProperties.Size;
        }

        #region Sending/receiving

        public async Task<OneFileTransferModel> SendFile(StorageFile file)
        {
            var fileSizeInBytes = await GetFileSizeInBytes(file);

            bool successfulFileSend;
            var fileInfo = _toxModel.FileSend(_friendNumber, ToxFileKind.Data, fileSizeInBytes, file.Name,
                out successfulFileSend);

            if (successfulFileSend)
            {
                var transferModel =
                    await OneFileTransferModel.CreateInstance(_dataService, _friendNumber, fileInfo.Number, file.Name,
                        fileSizeInBytes, TransferDirection.Up, file);
                return transferModel;
            }

            return null;
        }

        private async void FileSendRequestReceivedHandler(object sender,
            ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FileKind != ToxFileKind.Data || e.FriendNumber != _friendNumber)
                return;

            var fileId = _toxModel.FileGetId(e.FriendNumber, e.FileNumber);
            var resumeData = await _fileTransferResumer.GetDownloadData(fileId);

            OneFileTransferModel oneFileTransferModel;

            // If we could find the resume data for this download, we resume it instead of handling it as a new transfer.
            if (resumeData != null)
            {
                oneFileTransferModel =
                    await
                        OneBrokenFileTransferModel.CreateInstance(_dataService, e.FriendNumber, e.FileNumber,
                            resumeData.File.Name,
                            e.FileSize, TransferDirection.Down, resumeData.File, resumeData.TransferredBytes);
            }
            else
            {
                // We add a transfer with a null value instead of an actual stream here. We will replace it with an actual file stream
                // in OneFileTransferModel.AcceptTransfer() when the user accepts the request and chooses a location to save the file to.
                oneFileTransferModel =
                    await
                        OneFileTransferModel.CreateInstance(_dataService, e.FriendNumber, e.FileNumber, e.FileName,
                            e.FileSize,
                            TransferDirection.Down, null);
            }

            FileTransferAdded?.Invoke(this, oneFileTransferModel);
        }

        #endregion Sending/receiving

        #region File transfer resuming

        private async void FriendConnectionStatusChangedHandler(object sender,
            ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (_friendNumber != e.FriendNumber)
                return;

            if (_toxModel.IsFriendOnline(e.FriendNumber) &&
                _toxModel.LastConnectionStatusOfFriend(e.FriendNumber) == ToxConnectionStatus.None)
            {
                // The given friend just came online... let's restart all of our previously broken uploads towards him/her!
                await ResumeBrokenUploadsForFriend(e.FriendNumber);
            }
        }

        private async Task ResumeBrokenUploadsForFriend(int friendNumber)
        {
            var resumeDataOfBrokenUploads = await _fileTransferResumer.GetUploadData(friendNumber);

            foreach (var resumeData in resumeDataOfBrokenUploads)
            {
                var fileSizeInBytes = await GetFileSizeInBytes(resumeData.File);

                bool successfulFileSend;
                var fileInfo = _toxModel.FileSend(resumeData.FriendNumber, ToxFileKind.Data,
                    fileSizeInBytes, resumeData.File.Name, resumeData.FileId, out successfulFileSend);

                if (successfulFileSend)
                {
                    var oneFileTransferModel =
                        await
                            OneBrokenFileTransferModel.CreateInstance(_dataService, resumeData.FriendNumber,
                                fileInfo.Number,
                                resumeData.File.Name, fileSizeInBytes, TransferDirection.Up, resumeData.File,
                                resumeData.TransferredBytes);

                    FileTransferAdded?.Invoke(this, oneFileTransferModel);
                }
            }
        }

        #endregion File transfer resuming
    }
}