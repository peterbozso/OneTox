using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using SharpTox.Core;
using WinTox.Annotations;
using WinTox.ViewModel.FileTransfers;

namespace WinTox.Model
{
    public class OneFileTransferModel : INotifyPropertyChanged
    {
        private readonly int _fileNumber;
        private readonly int _friendNumber;
        private bool _isPlaceholder;
        private FileTransferState _state;
        private Stream _stream;
        public string Name;

        public OneFileTransferModel(int friendNumber, int fileNumber, string name, long fileSizeInBytes,
            TransferDirection direction, Stream stream, long transferredBytes = 0)
        {
            _stream = stream;
            if (_stream != null)
            {
                if (_stream.CanWrite)
                {
                    _stream.SetLength(fileSizeInBytes);
                }

                _stream.Position = transferredBytes;
            }
            Direction = direction;

            Name = name;

            _isPlaceholder = false;

            _friendNumber = friendNumber;
            _fileNumber = fileNumber;

            ToxModel.Instance.FileControlReceived += FileControlReceivedHandler;
            ToxModel.Instance.FileChunkRequested += FileChunkRequestedHandler;
            ToxModel.Instance.FileChunkReceived += FileChunkReceivedHandler;
        }

        public TransferDirection Direction { get; private set; }

        public double Progress
        {
            get
            {
                lock (_stream)
                {
                    return ((double) _stream.Position/_stream.Length)*100;
                }
            }
        }

        public FileTransferState State
        {
            get { return _state; }
            private set
            {
                if (value == _state)
                    return;
                _state = value;
                RaisePropertyChanged();

                //IsPlaceholder = value == FileTransferState.Finished || value == FileTransferState.Cancelled; TODO: Remove maybe?
                if (value == FileTransferState.Finished || value == FileTransferState.Cancelled)
                {
                    Dispose();
                }
            }
        }

        private void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            if (_isPlaceholder || !IsThisTransfer(e))
                return;

            Debug.WriteLine("File control received \t friend number: {0}, \t file number: {1}, \t control: {2}",
                e.FriendNumber, e.FileNumber, e.Control);

            switch (e.Control)
            {
                case ToxFileControl.Cancel:
                    State = FileTransferState.Cancelled;
                    return;
            }
        }

        #region Sending

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            if (_isPlaceholder || !IsThisTransfer(e))
                return;

            var chunk = GetNextChunk(e);
            var successfulChunkSend = ToxModel.Instance.FileSendChunk(e.FriendNumber, e.FileNumber, e.Position, chunk);

            if (successfulChunkSend)
            {
                if (IsFinished)
                {
                    State = FileTransferState.Finished;
                }
            }
        }

        #endregion

        #region Receiving

        private void FileChunkReceivedHandler(object sender, ToxEventArgs.FileChunkEventArgs e)
        {
            if (_isPlaceholder || !IsThisTransfer(e))
                return;

            PutNextChunk(e);

            if (IsFinished)
            {
                State = FileTransferState.Finished;
            }
        }

        #endregion

        #region Helpers

        private bool IsThisTransfer(ToxEventArgs.FileBaseEventArgs e)
        {
            return (e.FriendNumber == _friendNumber && e.FileNumber == _fileNumber);
        }

        #endregion

        #region Common transfer logic

        private long TransferredBytes
        {
            get { return _stream.Position; }
        }

        private bool IsFinished
        {
            get
            {
                lock (_stream)
                {
                    return _stream.Position == _stream.Length;
                }
            }
        }

        private byte[] GetNextChunk(ToxEventArgs.FileRequestChunkEventArgs e)
        {
            lock (_stream)
            {
                if (_stream.Position != e.Position)
                {
                    _stream.Seek(e.Position, SeekOrigin.Begin);
                }

                var chunk = new byte[e.Length];
                _stream.Read(chunk, 0, e.Length);

                return chunk;
            }
        }

        private void PutNextChunk(ToxEventArgs.FileChunkEventArgs e)
        {
            lock (_stream)
            {
                if (_stream.Position != e.Position)
                {
                    _stream.Seek(e.Position, SeekOrigin.Begin);
                }

                _stream.Write(e.Data, 0, e.Data.Length);
            }
        }

        private void Dispose()
        {
            if (_stream != null) // It could be a dummy transfer waiting for accept from the user!
            {
                _stream.Dispose();
                _stream = null;

                _isPlaceholder = true;
            }
        }

        #endregion

        #region Property changed event

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}