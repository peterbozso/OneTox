using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using OneTox.Config;
using SharpTox.Core;

namespace OneTox.Model.FileTransfers
{
    internal class OneBrokenFileTransferModel : OneFileTransferModel
    {
        private OneBrokenFileTransferModel(IDataService dataService, int friendNumber, int fileNumber, string name,
            long fileSizeInBytes,
            TransferDirection direction, Stream stream, long transferredBytes = 0)
            : base(dataService, friendNumber, fileNumber, name, fileSizeInBytes, direction, stream, transferredBytes)
        {
        }

        public new static async Task<OneFileTransferModel> CreateInstance(IDataService dataService, int friendNumber,
            int fileNumber, string name,
            long fileSizeInBytes, TransferDirection direction, StorageFile file, long transferredBytes = 0)
        {
            if (file != null)
            {
                dataService.FileTransferResumer.RecordTransfer(file, friendNumber, fileNumber, direction);
            }

            var fileStream = file == null ? null : await GetStreamBasedOnDirection(file, direction);

            var oneBrokenFileDownloadModel = new OneBrokenFileTransferModel(dataService, friendNumber, fileNumber, name,
                fileSizeInBytes, direction, fileStream, transferredBytes);

            if (direction == TransferDirection.Down)
            {
                dataService.ToxModel.FileSeek(friendNumber, fileNumber, transferredBytes);
                dataService.ToxModel.FileControl(friendNumber, fileNumber, ToxFileControl.Resume);
            }

            return oneBrokenFileDownloadModel;
        }

        protected override void SetInitialStateBasedOnDirection(TransferDirection direction)
        {
            switch (direction)
            {
                case TransferDirection.Up:
                    State = FileTransferState.Uploading;
                    break;

                case TransferDirection.Down:
                    State = FileTransferState.Downloading;
                    break;
            }
        }
    }
}