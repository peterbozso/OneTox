using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.AccessCache;
using OneTox.Model.Tox;

namespace OneTox.Model.FileTransfers
{
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

    /// <summary>
    ///     The data needed for resuming a previously broken file transfer.
    /// </summary>
    public class ResumeData
    {
        public StorageFile File;
        public byte[] FileId;
        public int FriendNumber;
        public long TransferredBytes;
    }

    /// <summary>
    ///     This class's responsibility is to keep record of broken file transfers between core restarts. It accomplishes this
    ///     goal by leveraging the benefits of future access list. We use it like this: whenever a file transfer added by
    ///     FileTransfersViewModel, we record that transfer in future access list with RecordTransfer(). If a transfer finishes
    ///     (due to really finishing it's progress, or because of being canceled by someone), it is removed from the future
    ///     access list. If one of the participants goes offline before a transfer finishes, we update it's progress with
    ///     UpdateTransfer() so we can resume the transfer where it is left off.
    /// </summary>
    internal class FileTransferResumer : IFileTransferResumer
    {
        private readonly StorageItemAccessList _futureAccesList = StorageApplicationPermissions.FutureAccessList;
        private readonly IToxModel _toxModel;

        public FileTransferResumer(IToxModel toxModel)
        {
            _toxModel = toxModel;

            _toxModel.FriendListChanged += FriendListChangedHandler;
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
                FileId = _toxModel.FileGetId(friendNumber, fileNumber),
                TransferredBytes = 0,
                Direction = direction
            };

            var xmlMetadata = SerializeMetadata(metadata);

            _futureAccesList.Add(file, xmlMetadata);
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

        private TransferMetadata DeserializeMetadata(string xml)
        {
            var deserializer = new XmlSerializer(typeof (TransferMetadata));
            var reader = new StringReader(xml);
            return (TransferMetadata) deserializer.Deserialize(reader);
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

        /// <summary>
        ///     In case a friend is removed from the friend list, we remove all broken transfers associated with him/her as well.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FriendListChangedHandler(object sender, FriendListChangedEventArgs e)
        {
            if (e.Action == FriendListChangedAction.Remove)
            {
                foreach (var entry in _futureAccesList.Entries)
                {
                    var metadata = DeserializeMetadata(entry.Metadata);

                    if (metadata.FriendNumber == e.FriendNumber)
                    {
                        _futureAccesList.Remove(entry.Token);
                    }
                }
            }
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
                _futureAccesList.Remove(token);

                var resumeData = new ResumeData
                {
                    FriendNumber = metadata.FriendNumber,
                    File = file,
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

        private string SerializeMetadata(TransferMetadata metadata)
        {
            var serializer = new XmlSerializer(typeof (TransferMetadata));
            var xmlMetadata = new StringBuilder();
            var writer = new StringWriter(xmlMetadata);
            serializer.Serialize(writer, metadata);
            return xmlMetadata.ToString();
        }
    }
}