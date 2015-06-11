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

        #region Common

        protected override void HandleFileControl(ToxFileControl fileControl, TransferId transferId)
        {
            switch (fileControl)
            {
                case ToxFileControl.Cancel:
                    ActiveTransfers.Remove(transferId);
                    Debug.WriteLine(
                        "Avatar transfer CANCELLED by friend! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                        transferId.FriendNumber, transferId.FileNumber, ActiveTransfers.Count);
                    return;
            }
        }

        #endregion

        #region Sending

        public void SendAvatar(int friendNumber, Stream stream, string fileName)
        {
            bool successfulFileSend;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, stream.Length, fileName,
                GetAvatarHash(stream), out successfulFileSend);

            if (successfulFileSend)
            {
                ActiveTransfers.Add(new TransferId(fileInfo.Number, friendNumber),
                    new TransferData(stream, stream.Length));
                Debug.WriteLine(
                    "Avatar upload added! \t friend number: {0}, \t file number: {1}, \t total avatar transfers: {2}",
                    friendNumber, fileInfo.Number, ActiveTransfers.Count);
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

        protected override void HandleFinishedUpload(TransferId transferId, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            ActiveTransfers.Remove(transferId);

            Debug.WriteLine(
                "Avatar upload removed! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                e.FriendNumber, e.FileNumber, ActiveTransfers.Count);
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

            var resumeSent = SendResumeControl(e.FriendNumber, e.FileNumber);
            if (resumeSent)
            {
                var stream = new MemoryStream((int) e.FileSize);
                ActiveTransfers.Add(new TransferId(e.FileNumber, e.FriendNumber),
                    new TransferData(stream, e.FileSize));

                Debug.WriteLine(
                    "Avatar download added! \t friend number: {0}, \t file number: {1}, \t total avatar transfers: {2}",
                    e.FriendNumber, e.FileNumber, ActiveTransfers.Count);
            }
        }

        private async Task<bool> AlreadyHaveAvatar(int friendNumber, int fileNumber)
        {
            var stream = await AvatarManager.Instance.GetFriendAvatarStream(friendNumber);
            if (stream == null)
                return false;
            var fileId = ToxModel.Instance.FileGetId(friendNumber, fileNumber);
            var avatarHash = GetAvatarHash(stream);
            return fileId.SequenceEqual(avatarHash);
        }

        protected override void HandleFinishedDownload(TransferId transferId, ToxEventArgs.FileChunkEventArgs e)
        {
            AvatarManager.Instance.ChangeFriendAvatar(e.FriendNumber, ActiveTransfers[transferId].Stream as MemoryStream);
            ActiveTransfers.Remove(transferId);

            Debug.WriteLine(
                "Avatar download removed! \t friend number: {0}, \t file number: {1}, \t total avatar transfers: {2}",
                e.FriendNumber, e.FileNumber, ActiveTransfers.Count);
        }

        #endregion
    }
}