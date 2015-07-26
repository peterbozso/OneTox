using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using SharpTox.Core;
using WinTox.Annotations;
using WinTox.ViewModel.FileTransfers;

namespace WinTox.Model
{
    public class OneFileTransferModel : INotifyPropertyChanged
    {
        #region Helpers

        private bool IsThisTransfer(ToxEventArgs.FileBaseEventArgs e)
        {
            return (e.FriendNumber == _friendNumber && e.FileNumber == _fileNumber);
        }

        #endregion

        #region Constructor

        public OneFileTransferModel(FileTransfersModel fileTransfersModel, int friendNumber, int fileNumber, string name,
            long fileSizeInBytes, TransferDirection direction, Stream stream, long transferredBytes = 0)
        {
            _fileTransfersModel = fileTransfersModel;

            _stream = stream;
            if (_stream != null)
            {
                if (_stream.CanWrite)
                {
                    _stream.SetLength(fileSizeInBytes);
                }

                _stream.Position = transferredBytes;
            }
            _fileSizeInBytes = fileSizeInBytes;

            _direction = direction;
            SetInitialStateBasedOnDirection(direction);

            Name = name;

            _isPlaceholder = false;

            _friendNumber = friendNumber;
            _fileNumber = fileNumber;

            ToxModel.Instance.FileControlReceived += FileControlReceivedHandler;
            ToxModel.Instance.FileChunkRequested += FileChunkRequestedHandler;
            ToxModel.Instance.FileChunkReceived += FileChunkReceivedHandler;
        }

        private void SetInitialStateBasedOnDirection(TransferDirection direction)
        {
            switch (direction)
            {
                case TransferDirection.Up:
                    State = FileTransferState.BeforeUpload;
                    break;
                case TransferDirection.Down:
                    State = FileTransferState.BeforeDownload;
                    break;
            }
        }

        #endregion

        #region Fields

        private readonly TransferDirection _direction;
        private readonly int _fileNumber;
        private readonly long _fileSizeInBytes;
        private readonly FileTransfersModel _fileTransfersModel;
        private readonly int _friendNumber;
        private bool _isPlaceholder;
        private FileTransferState _state;
        private Stream _stream;

        #endregion

        #region Properties

        public string Name { get; private set; }

        public double Progress
        {
            get
            {
                if (State == FileTransferState.Finished || State == FileTransferState.Cancelled)
                    return 100.0;

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

        #endregion

        #region Received file control handling

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
                case ToxFileControl.Pause:
                    TryPauseTransfer();
                    return;
                case ToxFileControl.Resume:
                    TryResumeTransfer();
                    return;
            }
        }

        private void TryPauseTransfer()
        {
            if (State != FileTransferState.Uploading && State != FileTransferState.Downloading)
                return;

            State = FileTransferState.PausedByFriend;
        }

        private void TryResumeTransfer()
        {
            if (State != FileTransferState.PausedByFriend && State != FileTransferState.BeforeUpload &&
                State != FileTransferState.BeforeDownload)
                return;

            SetResumingStateBasedOnDirection();
        }

        #endregion

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

        #endregion

        #region Control methods

        public bool CancelTransfer()
        {
            var successfulSend = ToxModel.Instance.FileControl(_friendNumber, _fileNumber, ToxFileControl.Cancel);

            if (successfulSend)
            {
                _fileTransfersModel.Transfers.Remove(this);
            }

            return successfulSend;
        }

        public async Task AcceptTransfer(StorageFile file)
        {
            var fileStream = (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStream();

            // Replace the dummy stream set previously in FileSendRequestReceivedHandler():
            ReplaceStream(fileStream);

            var successfulSend = ToxModel.Instance.FileControl(_friendNumber, _fileNumber, ToxFileControl.Resume);

            if (successfulSend)
            {
                // FileTransferResumer.Instance.RecordTransfer(file, FriendNumber, fileNumber, TransferDirection.Down); TODO!
                Debug.WriteLine("STUB: AcceptTransfer()!");

                State = FileTransferState.Downloading;
                return;
            }

            ReplaceStream(null);
            fileStream.Dispose();
        }

        public void PauseTransfer()
        {
            if (State != FileTransferState.Downloading && State != FileTransferState.Uploading)
                return;

            var successfulSend = ToxModel.Instance.FileControl(_friendNumber, _fileNumber, ToxFileControl.Pause);

            if (successfulSend)
            {
                State = FileTransferState.PausedByUser;
            }
        }

        public void ResumeTransfer()
        {
            if (State != FileTransferState.PausedByUser)
                return;

            var successfulSend = ToxModel.Instance.FileControl(_friendNumber, _fileNumber, ToxFileControl.Resume);

            if (successfulSend)
            {
                SetResumingStateBasedOnDirection();
            }
        }

        #endregion

        #region Common

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

        private void Dispose()
        {
            if (_stream != null) // It could be a dummy transfer waiting for accept from the user!
            {
                _stream.Dispose();
                _stream = null;

                _isPlaceholder = true;
            }
        }

        private void SetResumingStateBasedOnDirection()
        {
            switch (_direction)
            {
                case TransferDirection.Up:
                    State = FileTransferState.Uploading;
                    break;
                case TransferDirection.Down:
                    State = FileTransferState.Downloading;
                    break;
            }
        }

        private void ReplaceStream(Stream newStream)
        {
            if (_stream != null) // We only allow replacement of a dummy stream.
                return;

            newStream.SetLength(_fileSizeInBytes);
            _stream = newStream;
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