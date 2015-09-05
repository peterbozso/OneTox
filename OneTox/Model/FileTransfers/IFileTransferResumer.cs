using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace OneTox.Model.FileTransfers
{
    public interface IFileTransferResumer
    {
        Task<ResumeData> GetDownloadData(byte[] fileId);

        Task<List<ResumeData>> GetUploadData(int friendNumber);

        void RecordTransfer(StorageFile file, int friendNumber, int fileNumber, TransferDirection direction);

        void RemoveTransfer(int friendNumber, int fileNumber);

        Task UpdateTransfer(int friendNumber, int fileNumber, long transferredBytes);
    }
}