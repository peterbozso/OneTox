using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace WinTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    internal class FileTransferResumer
    {
        private static FileTransferResumer _instace;
        private readonly StorageItemAccessList _futureAccesList = StorageApplicationPermissions.FutureAccessList;

        private FileTransferResumer()
        {
            _futureAccesList.Clear(); // TODO: Remove!
        }

        public static FileTransferResumer Instance
        {
            get { return _instace ?? (_instace = new FileTransferResumer()); }
        }

        public void RecordTransfer(StorageFile file, int friendNumber, int fileNumber)
        {
            // TODO: Maybe we should try to make place for newer items?
            if (_futureAccesList.MaximumItemsAllowed == _futureAccesList.Entries.Count)
                return;

            var onlyFileIdMetaData = GetFileIdAsString(friendNumber, fileNumber);

            _futureAccesList.Add(file, onlyFileIdMetaData);
        }

        public async Task ConfirmTransfer(int friendNumber, int fileNumber, long transferredBytes)
        {
            var token = FindEntry(friendNumber, fileNumber);
            var file = await _futureAccesList.GetFileAsync(token);
            var metadata = SerializeMetadata(friendNumber, fileNumber, transferredBytes);
            _futureAccesList.AddOrReplace(token, file, metadata);
        }

        public void RemoveTransfer(int friendNumber, int fileNumber)
        {
            var token = FindEntry(friendNumber, fileNumber);

            if (token != String.Empty)
                _futureAccesList.Remove(token);
        }

        // We use it only when the metadata is just the fileId in string format so it's all okay like this.
        private string FindEntry(int friendNumber, int fileNumber)
        {
            var onlyFileIdMetaData = GetFileIdAsString(friendNumber, fileNumber);

            foreach (var entry in _futureAccesList.Entries)
            {
                if (entry.Metadata == onlyFileIdMetaData)
                {
                    return entry.Token;
                }
            }

            return String.Empty;
        }

        private string SerializeMetadata(int friendNumber, int fileNumber, long transferredBytes)
        {
            var serializer = new XmlSerializer(typeof (TransferMetadata));
            var xmlMetadata = new StringBuilder();
            var writer = new StringWriter(xmlMetadata);
            var metadata = new TransferMetadata
            {
                FileId = ToxModel.Instance.FileGetId(friendNumber, fileNumber),
                TransferredBytes = transferredBytes
            };
            serializer.Serialize(writer, metadata);
            return xmlMetadata.ToString();
        }

        private string GetFileIdAsString(int friendNumber, int fileNumber)
        {
            var fileId = ToxModel.Instance.FileGetId(friendNumber, fileNumber);
            return Encoding.UTF8.GetString(fileId, 0, fileId.Length);
        }
    }

    public struct TransferMetadata
    {
        public byte[] FileId;
        public long TransferredBytes;
    }
}