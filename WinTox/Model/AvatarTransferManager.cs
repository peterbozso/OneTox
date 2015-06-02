using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
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

        #region Sending

        public async Task SendAvatar(int friendNumber, StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();

            ToxErrorFileSend error;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, stream.Length, file.Name,
                GetAvatarHash(stream), out error);

            if (error == ToxErrorFileSend.Ok)
            {
                ActiveTransfers.Add(new TransferId(fileInfo.Number, friendNumber),
                    new TransferData(ToxFileKind.Avatar, stream, stream.Length));
                Debug.WriteLine(
                    "Avatar upload added! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                    friendNumber, fileInfo.Number, ActiveTransfers.Count);
            }
            // TODO: Error handling!
        }

        public void SendNullAvatar(int friendNumber)
        {
            ToxErrorFileSend error;
            ToxModel.Instance.FileSend(friendNumber, ToxFileKind.Avatar, 0, "", GetAvatarHash(new MemoryStream()),
                out error);
            // TODO: Error handling!
        }

        private byte[] GetAvatarHash(Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int) stream.Length);
            return ToxTools.Hash(buffer);
        }

        #endregion

        #region Receiving

        protected override async void FileSendRequestReceivedHandler(object sender,
            ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FileKind == ToxFileKind.Avatar)
            {
                await ReceiveAvatar(e);
            }
        }

        private async Task ReceiveAvatar(ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FileKind == ToxFileKind.Avatar && e.FileSize == 0) // It means the avatar of the friend is removed.
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
                    new TransferData(ToxFileKind.Avatar, stream, e.FileSize));

                Debug.WriteLine(
                    "Avatar download added! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                    e.FriendNumber, e.FileNumber, ActiveTransfers.Count);
            }
        }

        private async Task<bool> AlreadyHaveAvatar(int friendNumber, int fileNumber)
        {
            var fileId = ToxModel.Instance.FileGetId(friendNumber, fileNumber);
            var friendAvatarFile = await AvatarManager.Instance.GetFriendAvatarFile(friendNumber);
            var stream = (await friendAvatarFile.OpenReadAsync()).AsStreamForRead();
            var avatarHash = GetAvatarHash(stream);
            return fileId.SequenceEqual(avatarHash);
        }

        protected override void HandleFinishedDownload(TransferId transferId, ToxEventArgs.FileChunkEventArgs e)
        {
            AvatarManager.Instance.ChangeFriendAvatar(e.FriendNumber, ActiveTransfers[transferId].Stream as MemoryStream);
            ActiveTransfers.Remove(transferId);

            Debug.WriteLine(
                "Avatar download removed! \t friend number: {0}, \t file number: {1}, \t total transfers: {2}",
                e.FriendNumber, e.FileNumber, ActiveTransfers.Count);
        }

        #endregion
    }
}