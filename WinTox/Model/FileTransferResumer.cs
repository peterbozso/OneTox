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

        public async Task ConfirmTransfer(int friendNumber, int fileNumber, long transferredBytes)
        {
            var entry = FindEntry(friendNumber, fileNumber);
            if (entry.Token == null)
                return;

            var file = await _futureAccesList.GetFileAsync(entry.Token);
            var metadata = DeserializeMetadata(entry.Metadata);
            metadata.TransferredBytes = transferredBytes;

            _futureAccesList.AddOrReplace(entry.Token, file, SerializeMetadata(metadata));
        }

        public async Task<List<ResumeData>> GetResumeDataOfSavedUploadsForFriend(int friendNumber)
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

        public bool IsFileIdSaved(byte[] fileId)
        {
            foreach (var entry in _futureAccesList.Entries)
            {
                var metadata = DeserializeMetadata(entry.Metadata);
                if (metadata.FileId.SequenceEqual(fileId))
                    return true;
            }
            return false;
        }

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
            var fileId = ToxModel.Instance.FileGetId(e.FriendNumber, e.FileNumber);
            var entry = FindEntry(fileId);

            if (entry.Token != null)
                _futureAccesList.Remove(entry.Token);
        }

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

        private AccessListEntry FindEntry(byte[] fileId)
        {
            foreach (var entry in _futureAccesList.Entries)
            {
                var metadata = DeserializeMetadata(entry.Metadata);

                if (metadata.FileId.SequenceEqual(fileId))
                {
                    return entry;
                }
            }

            return new AccessListEntry();
        }

        private string SerializeMetadata(TransferMetadata metadata)
        {
            var serializer = new XmlSerializer(typeof(TransferMetadata));
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

    public class ResumeData
    {
        public int FriendNumber;
        public Stream FileStream;
        public string FileName;
        public byte[] FileId;
        public long TransferredBytes;
    }

    public struct TransferMetadata
    {
        public byte[] FileId;
        public int FileNumber;
        public int FriendNumber;
        public long TransferredBytes;
        public TransferDirection Direction;
    }
}