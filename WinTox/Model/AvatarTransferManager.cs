using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpTox.Core;

namespace WinTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class AvatarTransferManager : DataTransferManager
    {
        private static AvatarTransferManager _instance;

        public static AvatarTransferManager Instance
        {
            get { return _instance ?? (_instance = new AvatarTransferManager()); }
        }

        #region Debug

        protected void AddTransfer(int friendNumber, int fileNumber, Stream stream, long dataSizeInBytes,
            TransferDirection direction)
        {
            base.AddTransfer(friendNumber, fileNumber, stream, dataSizeInBytes, direction);

            Debug.WriteLine(
                "Avatar {0}load added! \t friend number: {1}, \t file number: {2}, \t total avatar transfers: {3}",
                direction, friendNumber, fileNumber, Transfers.Count);
        }

        protected new void RemoveTransfer(TransferId transferId)
        {
            var direction = Transfers[transferId].Direction;

            base.RemoveTransfer(transferId);

            Debug.WriteLine(
                "Avatar {0}load removed! \t friend number: {1}, \t file number: {2}, \t total avatar transfers: {3}",
                direction, transferId.FriendNumber, transferId.FileNumber, Transfers.Count);
        }

        #endregion

        #region Common

        protected override void HandleFileControl(ToxFileControl fileControl, TransferId transferId)
        {
            switch (fileControl)
            {
                case ToxFileControl.Cancel:
                    RemoveTransfer(transferId);
                    return;
            }
        }

        /// <summary>
        ///     By calling this function before sending or receiving an avatar, we ensure that there is only
        ///     1 upload and/or 1 download per friend at the same time.
        /// </summary>
        /// <param name="friendNumber">The friendNumber of the friend we'd like to remove transfers of.</param>
        /// <param name="direction">The direction of the transfers we'd like to remove.</param>
        private void RemoveAllTranfersOfFriendPerDirection(int friendNumber, TransferDirection direction)
        {
            var transfers = Transfers.ToArray();
            foreach (var transfer in transfers)
            {
                if (transfer.Key.FriendNumber == friendNumber && transfer.Value.Direction == direction)
                {
                    SendCancelControl(transfer.Key.FriendNumber, transfer.Key.FileNumber);
                    RemoveTransfer(transfer.Key);
                }
            }
        }

        #endregion

        #region Sending

        public void SendAvatar(int friendNumber, Stream stream, string fileName)
        {
            RemoveAllTranfersOfFriendPerDirection(friendNumber, TransferDirection.Up);

            bool successfulFileSend;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, stream.Length, fileName,
                GetAvatarHash(stream), out successfulFileSend);

            if (successfulFileSend)
            {
                AddTransfer(friendNumber, fileInfo.Number, stream, stream.Length, TransferDirection.Up);
            }
            else
            {
                stream.Dispose();
            }
        }

        public void SendNullAvatar(int friendNumber)
        {
            bool successfulFileSend;
            ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, 0, "", out successfulFileSend);
        }

        private byte[] GetAvatarHash(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            return ToxTools.Hash(buffer);
        }

        protected override void HandleFinishedUpload(TransferId transferId, int friendNumber, int fileNumber)
        {
            RemoveTransfer(transferId);
        }

        #endregion

        #region Receiving

        protected override async void FileSendRequestReceivedHandler(object sender,
            ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FileKind != ToxFileKind.Avatar)
                return;

            if (e.FileSize == 0) // It means the avatar of the friend is removed.
            {
                SendCancelControl(e.FriendNumber, e.FileNumber);
                await AvatarManager.Instance.RemoveFriendAvatar(e.FriendNumber);
                return;
            }

            if (await AlreadyHaveAvatar(e.FriendNumber, e.FileNumber))
            {
                SendCancelControl(e.FriendNumber, e.FileNumber);
                return;
            }

            RemoveAllTranfersOfFriendPerDirection(e.FriendNumber, TransferDirection.Down);

            var resumeSent = SendResumeControl(e.FriendNumber, e.FileNumber);
            if (resumeSent)
            {
                var stream = new MemoryStream((int) e.FileSize);
                AddTransfer(e.FriendNumber, e.FileNumber, stream, e.FileSize, TransferDirection.Down);
            }
        }

        private async Task<bool> AlreadyHaveAvatar(int friendNumber, int fileNumber)
        {
            using (var stream = await AvatarManager.Instance.GetFriendAvatarStream(friendNumber))
            {
                if (stream == null)
                    return false;
                var fileId = ToxModel.Instance.FileGetId(friendNumber, fileNumber);
                var avatarHash = GetAvatarHash(stream);
                return fileId.SequenceEqual(avatarHash);
            }
        }

        protected override void HandleFinishedDownload(TransferId transferId, int friendNumber, int fileNumber)
        {
            AvatarManager.Instance.ChangeFriendAvatar(friendNumber, Transfers[transferId].GetMemoryStream());
            RemoveTransfer(transferId);
        }

        #endregion
    }
}