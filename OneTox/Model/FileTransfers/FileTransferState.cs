namespace OneTox.Model.FileTransfers
{
    public enum FileTransferState
    {
        BeforeUpload,
        BeforeDownload,
        Uploading,
        Downloading,
        PausedByUser,
        PausedByFriend,
        Finished,
        Cancelled
    }
}