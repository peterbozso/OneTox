using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpTox.Core;

namespace OneTox.Model.FileTransfers
{
    internal class OneBrokenFileTransferModel : OneFileTransferModel
    {
        private OneBrokenFileTransferModel(int friendNumber, int fileNumber, string name, long fileSizeInBytes,
            TransferDirection direction, Stream stream, long transferredBytes = 0)
            : base(friendNumber, fileNumber, name, fileSizeInBytes, direction, stream, transferredBytes)
        {
        }

        public new static async Task<OneFileTransferModel> CreateInstance(int friendNumber, int fileNumber, string name,
            long fileSizeInBytes, TransferDirection direction, StorageFile file, long transferredBytes = 0)
        {
            if (file != null)
                FileTransferResumer.Instance.RecordTransfer(file, friendNumber, fileNumber, direction);

            var fileStream = file == null ? null : await GetStreamBasedOnDirection(file, direction);

            var oneBrokenFileDownloadModel = new OneBrokenFileTransferModel(friendNumber, fileNumber, name,
                fileSizeInBytes, direction, fileStream, transferredBytes);

            if (direction == TransferDirection.Down)
            {
                ToxModel.Instance.FileSeek(friendNumber, fileNumber, transferredBytes);
                ToxModel.Instance.FileControl(friendNumber, fileNumber, ToxFileControl.Resume);
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