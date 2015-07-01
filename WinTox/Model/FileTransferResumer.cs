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
            FileTransferManager.Instance.TransferFinished += TransferFinishedHandler;

            _futureAccesList.Clear(); // TODO: Remove!
        }

        private void TransferFinishedHandler(object sender, FileTransferManager.TransferFinishedEventArgs e)
        {
            var token = FindEntry(e.FriendNumber, e.FileNumber);

            if (token != String.Empty)
                _futureAccesList.Remove(token);
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

            var metadata = SerializeMetadata(friendNumber, fileNumber, 0);

            _futureAccesList.Add(file, metadata);
        }

        public async Task ConfirmTransfer(int friendNumber, int fileNumber, long transferredBytes)
        {
            var token = FindEntry(friendNumber, fileNumber);
            if (token == String.Empty)
                return;

            var file = await _futureAccesList.GetFileAsync(token);
            var metadata = SerializeMetadata(friendNumber, fileNumber, transferredBytes);

            _futureAccesList.AddOrReplace(token, file, metadata);
        }

        private string FindEntry(int friendNumber, int fileNumber)
        {
            foreach (var entry in _futureAccesList.Entries)
            {
                var metadata = DeserializeMetadata(entry.Metadata);

                if (metadata.FriendNumber == friendNumber && metadata.FriendNumber == fileNumber)
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
                FriendNumber = friendNumber,
                FileNumber = fileNumber,
                FileId = ToxModel.Instance.FileGetId(friendNumber, fileNumber),
                TransferredBytes = transferredBytes
            };
            serializer.Serialize(writer, metadata);
            return xmlMetadata.ToString();
        }

        private TransferMetadata DeserializeMetadata(string xaml)
        {
            var deserializer = new XmlSerializer(typeof(TransferMetadata));
            var reader = new StringReader(xaml);
            return (TransferMetadata) deserializer.Deserialize(reader);
        }
    }

    public struct TransferMetadata
    {
        public int FriendNumber;
        public int FileNumber;
        public byte[] FileId;
        public long TransferredBytes;
    }
}