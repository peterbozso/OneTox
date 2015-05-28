using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Streams;
using SharpTox.Core;
using System.Threading.Tasks;

namespace WinTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class FileTransferManager
    {
        private static FileTransferManager _instance;

        private readonly Dictionary<ToxFileInfo, IRandomAccessStream> _filesBeingSent; 

        private FileTransferManager()
        {
            _filesBeingSent = new Dictionary<ToxFileInfo, IRandomAccessStream>();
            ToxModel.Instance.FileControlReceived += FileControlReceivedHandler;
            ToxModel.Instance.FileChunkRequested += FileChunkRequestedHandler;
        }

        private void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            throw new NotImplementedException();
        }

        public async Task Send(int friendNumber, ToxFileKind kind,  StorageFile file)
        {
            var stream = await file.OpenReadAsync();
            ToxErrorFileSend error;
            var fileInfo = ToxModel.Instance.FileSend(friendNumber, kind, (long) stream.Size, file.Name, out error);
            if (error == ToxErrorFileSend.Ok)
            {
                _filesBeingSent.Add(fileInfo, stream);
            }
            else
            {
                // TODO: Handle error!
            }
        }

        public static FileTransferManager Instance
        {
            get { return _instance ?? (_instance = new FileTransferManager()); }
        }
    }
}