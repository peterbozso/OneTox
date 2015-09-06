using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace OneTox.Model.FileTransfers
{
    internal class MockFileTransferResumer : IFileTransferResumer
    {
        public Task<ResumeData> GetDownloadData(byte[] fileId)
        {
            return null;
        }

        public Task<List<ResumeData>> GetUploadData(int friendNumber)
        {
            return Task.FromResult(new List<ResumeData>());
        }

        public void RecordTransfer(StorageFile file, int friendNumber, int fileNumber, TransferDirection direction)
        {
        }

        public void RemoveTransfer(int friendNumber, int fileNumber)
        {
        }

        public Task UpdateTransfer(int friendNumber, int fileNumber, long transferredBytes)
        {
            return Task.FromResult(new object());
        }
    }
}