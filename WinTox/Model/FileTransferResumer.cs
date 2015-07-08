using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace WinTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    ///     This class's responsibility is to keep record of broken file transfers between core restarts. It accomplishes this
    ///     goal by leveraging the benefits of future access list.
    /// </summary>
    internal class FileTransferResumer
    {
        private static FileTransferResumer _instace;
        private readonly StorageItemAccessList _futureAccesList = StorageApplicationPermissions.FutureAccessList;

        private FileTransferResumer()
        {
            FileTransferManager.Instance.TransferFinished += TransferFinishedHandler;
        }

        public static FileTransferResumer Instance
        {
            get { return _instace ?? (_instace = new FileTransferResumer()); }
        }

        /// <summary>
        ///     Records a file transfer for future resuming between core restarts.
        /// </summary>
        /// <param name="file">The file associated with the transfer.</param>
        /// <param name="friendNumber">The friend number of the transfer.</param>
        /// <param name="fileNumber">The file number of the transfer.</param>
        /// <param name="direction">The direction of the transfer.</param>
        public void RecordTransfer(StorageFile file, int friendNumber, int fileNumber, TransferDirection direction)
        {
            // TODO: Maybe we should try to make place for newer items?
            if (_futureAccesList.MaximumItemsAllowed == _futureAccesList.Entries.Count)
                return;

            var metadata = new TransferMetadata
            {
                FriendNumber = friendNumber,
                FileNumber = fileNumber,
                FileId = ToxModel.Instance.FileGetId(friendNumber, fileNumber),
                TransferredBytes = 0,
                Direction = direction
            };

            var xmlMetadata = SerializeMetadata(metadata);

            _futureAccesList.Add(file, xmlMetadata);
        }

        /// <summary>
        ///     Updates the amount of transferred bytes we store for a transfer that was recorded previously with RecordTransfer().
        /// </summary>
        /// <param name="friendNumber">Friend number of the transfer to update.</param>
        /// <param name="fileNumber">File number of the transfer to update.</param>
        /// <param name="transferredBytes">New amount of transferred bytes.</param>
        /// <returns></returns>
        public async Task UpdateTransfer(int friendNumber, int fileNumber, long transferredBytes)
        {
            var entry = FindEntry(friendNumber, fileNumber);
            if (entry.Token == null)
                return;

            var file = await _futureAccesList.GetFileAsync(entry.Token);
            var metadata = DeserializeMetadata(entry.Metadata);
            metadata.TransferredBytes = transferredBytes;

            _futureAccesList.AddOrReplace(entry.Token, file, SerializeMetadata(metadata));
        }

        /// <summary>
        ///     Removes the record of the given file transfer.
        /// </summary>
        /// <param name="friendNumber">The friend number of the transfer</param>
        /// <param name="fileNumber">The file number of the transfer</param>
        public void RemoveTransfer(int friendNumber, int fileNumber)
        {
            var entry = FindEntry(friendNumber, fileNumber);

            if (entry.Token != null)
                _futureAccesList.Remove(entry.Token);
        }

        /// <summary>
        ///     Retrieves the necessary data to resume all broken file uploads for a given friend.
        /// </summary>
        /// <param name="friendNumber">The friend number of the friend to retrieve the data for.</param>
        /// <returns>The list of data necessary for resuming the transfers.</returns>
        public async Task<List<ResumeData>> GetUploadData(int friendNumber)
        {
            var resumeDataOfSavedUploads = new List<ResumeData>();
            var entries = _futureAccesList.Entries.ToArray();

            foreach (var entry in entries)
            {
                var metadata = DeserializeMetadata(entry.Metadata);

                if (metadata.Direction != TransferDirection.Up || metadata.FriendNumber != friendNumber)
                    continue;

                var resumeData = await GetResumeData(entry.Token, metadata);
                if (resumeData == null)
                    continue;

                resumeDataOfSavedUploads.Add(resumeData);
            }

            return resumeDataOfSavedUploads;
        }

        /// <summary>
        ///     Retrieves the necessary data to resume a broken file download based on a file ID.
        /// </summary>
        /// <param name="fileId">The file ID to search for.</param>
        /// <returns>The data necessary for resuming the transfer.</returns>
        public async Task<ResumeData> GetDownloadData(byte[] fileId)
        {
            foreach (var entry in _futureAccesList.Entries)
            {
                var metadata = DeserializeMetadata(entry.Metadata);

                if (metadata.FileId.SequenceEqual(fileId))
                {
                    return await GetResumeData(entry.Token, metadata);
                }
            }
            return null;
        }

        /// <summary>
        ///     Constructs and returns resume data (based on metadata) necessary for a transfer to resume it.
        /// </summary>
        /// <param name="token">The token of the record in the future access list.</param>
        /// <param name="metadata">The metadata of the transfer to get the resume data for.</param>
        /// <returns>The resume data.</returns>
        private async Task<ResumeData> GetResumeData(string token, TransferMetadata metadata)
        {
            try
            {
                var file = await _futureAccesList.GetFileAsync(token);
                var stream = await GetStreamBasedOnDirection(file, metadata.Direction);
                var resumeData = new ResumeData
                {
                    FriendNumber = metadata.FriendNumber,
                    FileStream = stream,
                    FileName = file.Name,
                    FileId = metadata.FileId,
                    TransferredBytes = metadata.TransferredBytes
                };

                return resumeData;
            }
            catch (FileNotFoundException)
            {
                // If we don't find the file anymore, we really don't need to keep record of it.
                _futureAccesList.Remove(token);
            }
            return null;
        }

        private async Task<Stream> GetStreamBasedOnDirection(StorageFile file, TransferDirection direction)
        {
            switch (direction)
            {
                case TransferDirection.Up:
                    return await file.OpenStreamForReadAsync();
                case TransferDirection.Down:
                    return await file.OpenStreamForWriteAsync();
            }
            return null;
        }

        private void TransferFinishedHandler(object sender, FileTransferManager.TransferFinishedEventArgs e)
        {
            RemoveTransfer(e.FriendNumber, e.FileNumber);
        }

        /// <summary>
        ///     Finds an entry in the future access list.
        /// </summary>
        /// <param name="friendNumber">The friend number to look for.</param>
        /// <param name="fileNumber">The file number to look for.</param>
        /// <returns>The entry we are looking for.</returns>
        private AccessListEntry FindEntry(int friendNumber, int fileNumber)
        {
            foreach (var entry in _futureAccesList.Entries)
            {
                var metadata = DeserializeMetadata(entry.Metadata);

                if (metadata.FriendNumber == friendNumber && metadata.FileNumber == fileNumber)
                {
                    return entry;
                }
            }

            return new AccessListEntry();
        }

        private string SerializeMetadata(TransferMetadata metadata)
        {
            var serializer = new XmlSerializer(typeof (TransferMetadata));
            var xmlMetadata = new StringBuilder();
            var writer = new StringWriter(xmlMetadata);
            serializer.Serialize(writer, metadata);
            return xmlMetadata.ToString();
        }

        private TransferMetadata DeserializeMetadata(string xaml)
        {
            var deserializer = new XmlSerializer(typeof (TransferMetadata));
            var reader = new StringReader(xaml);
            return (TransferMetadata) deserializer.Deserialize(reader);
        }
    }

    /// <summary>
    ///     The data needed for resuming a previously broken file transfer.
    /// </summary>
    public class ResumeData
    {
        public byte[] FileId;
        public string FileName;
        public Stream FileStream;
        public int FriendNumber;
        public long TransferredBytes;
    }

    /// <summary>
    ///     The data needed to be stored for each entry in the future access list.
    /// </summary>
    public struct TransferMetadata
    {
        public TransferDirection Direction;
        public byte[] FileId;
        public int FileNumber;
        public int FriendNumber;
        public long TransferredBytes;
    }
}