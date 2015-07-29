using System.IO;
using SharpTox.Core;

namespace OneTox.Model
{
    public class OneBrokenFileTransferModel : OneFileTransferModel
    {
        public OneBrokenFileTransferModel(int friendNumber, int fileNumber,
            string name, long fileSizeInBytes, TransferDirection direction, Stream stream, long transferredBytes = 0)
            : base(friendNumber, fileNumber, name, fileSizeInBytes, direction, stream, transferredBytes)
        {
        }

        public bool ResumeBrokenTransfer()
        {
            var successFulSend = ToxModel.Instance.FileSeek(FriendNumber, FileNumber, FileSizeInBytes);
            if (!successFulSend)
                return false;

            successFulSend = ToxModel.Instance.FileControl(FriendNumber, FileNumber, ToxFileControl.Resume);
            if (!successFulSend)
                return false;

            State = FileTransferState.Downloading; // TODO: We should be able to resume not just broken downloads!
            return true;
        }
    }
}