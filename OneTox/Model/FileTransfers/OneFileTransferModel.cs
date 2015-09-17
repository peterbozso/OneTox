using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using OneTox.Config;
using OneTox.Model.Tox;
using SharpTox.Core;

namespace OneTox.Model.FileTransfers
{
    public class OneFileTransferModel : ObservableObject
    {
        #region Helpers

        private bool IsThisTransfer(ToxEventArgs.FileBaseEventArgs e)
        {
            return (e.FriendNumber == _friendNumber && e.FileNumber == _fileNumber);
        }

        #endregion Helpers

        #region Constructor

        protected OneFileTransferModel(IDataService dataService, int friendNumber, int fileNumber, string name,
            long fileSizeInBytes, TransferDirection direction, Stream stream, long transferredBytes = 0)
        {
            _toxModel = dataService.ToxModel;
            _fileTransferResumer = dataService.FileTransferResumer;

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

            _friendNumber = friendNumber;
            _fileNumber = fileNumber;

            _toxModel.FileControlReceived += FileControlReceivedHandler;
            _toxModel.FileChunkRequested += FileChunkRequestedHandler;
            _toxModel.FileChunkReceived += FileChunkReceivedHandler;
            _toxModel.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
            Application.Current.Suspending += AppSuspendingHandler;
        }

        public static async Task<OneFileTransferModel> CreateInstance(IDataService dataService, int friendNumber,
            int fileNumber, string name,
            long fileSizeInBytes, TransferDirection direction, StorageFile file, long transferredBytes = 0)
        {
            if (file != null)
            {
                dataService.FileTransferResumer.RecordTransfer(file, friendNumber, fileNumber, direction);
            }

            var fileStream = file == null ? null : await GetStreamBasedOnDirection(file, direction);

            return new OneFileTransferModel(dataService, friendNumber, fileNumber, name, fileSizeInBytes, direction,
                fileStream,
                transferredBytes);
        }

        protected static async Task<Stream> GetStreamBasedOnDirection(StorageFile file, TransferDirection direction)
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

        protected virtual void SetInitialStateBasedOnDirection(TransferDirection direction)
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

        #endregion Constructor

        #region Fields

        private readonly TransferDirection _direction;
        private readonly int _fileNumber;
        private readonly long _fileSizeInBytes;
        private readonly int _friendNumber;
        private FileTransferState _state;
        private Stream _stream;
        private readonly IToxModel _toxModel;
        private readonly IFileTransferResumer _fileTransferResumer;

        #endregion Fields

        #region Properties

        public string Name { get; private set; }

        public double Progress
        {
            get
            {
                if (IsPlaceholder)
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
            protected set
            {
                Set(ref _state, value);

                if (IsPlaceholder)
                {
                    Dispose();
                }
            }
        }

        private bool IsPlaceholder => State == FileTransferState.Finished || State == FileTransferState.Cancelled;

        #endregion Properties

        #region Received file control handling

        private void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            if (IsPlaceholder || !IsThisTransfer(e))
                return;

            switch (e.Control)
            {
                case ToxFileControl.Cancel:
                    _fileTransferResumer.RemoveTransfer(_friendNumber, _fileNumber);
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

        #endregion Received file control handling

        #region Sending

        private void FileChunkRequestedHandler(object sender, ToxEventArgs.FileRequestChunkEventArgs e)
        {
            if (IsPlaceholder || !IsThisTransfer(e))
                return;

            var chunk = GetNextChunk(e);
            var successfulChunkSend = _toxModel.FileSendChunk(e.FriendNumber, e.FileNumber, e.Position, chunk);

            if (successfulChunkSend)
            {
                if (IsFinished)
                {
                    _fileTransferResumer.RemoveTransfer(_friendNumber, _fileNumber);
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

        #endregion Sending

        #region Receiving

        private void FileChunkReceivedHandler(object sender, ToxEventArgs.FileChunkEventArgs e)
        {
            if (IsPlaceholder || !IsThisTransfer(e))
                return;

            PutNextChunk(e);

            if (IsFinished)
            {
                _fileTransferResumer.RemoveTransfer(_friendNumber, _fileNumber);
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

        #endregion Receiving

        #region Control methods

        public async Task AcceptTransfer(StorageFile file)
        {
            var fileStream = (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStream();

            // Replace the dummy stream set previously in FileSendRequestReceivedHandler():
            ReplaceStream(fileStream);

            var successfulSend = _toxModel.FileControl(_friendNumber, _fileNumber, ToxFileControl.Resume);

            if (successfulSend)
            {
                _fileTransferResumer.RecordTransfer(file, _friendNumber, _fileNumber, TransferDirection.Down);
                State = FileTransferState.Downloading;
            }
            else
            {
                ReplaceStream(null);
                fileStream.Dispose();
            }
        }

        public void CancelTransfer()
        {
            _fileTransferResumer.RemoveTransfer(_friendNumber, _fileNumber);

            if (!IsPlaceholder)
                _toxModel.FileControl(_friendNumber, _fileNumber, ToxFileControl.Cancel);
        }

        public void PauseTransfer()
        {
            if (State != FileTransferState.Downloading && State != FileTransferState.Uploading)
                return;

            var successfulSend = _toxModel.FileControl(_friendNumber, _fileNumber, ToxFileControl.Pause);

            if (successfulSend)
            {
                State = FileTransferState.PausedByUser;
            }
        }

        public void ResumeTransfer()
        {
            if (State != FileTransferState.PausedByUser)
                return;

            var successfulSend = _toxModel.FileControl(_friendNumber, _fileNumber, ToxFileControl.Resume);

            if (successfulSend)
            {
                SetResumingStateBasedOnDirection();
            }
        }

        #endregion Control methods

        #region Common

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

        private long TransferredBytes => _stream?.Position ?? 0;

        private void Dispose()
        {
            if (_stream != null) // It could be a dummy transfer waiting for accept from the user!
            {
                _stream.Dispose();
                _stream = null;
            }
        }

        private void ReplaceStream(Stream newStream)
        {
            if (_stream != null) // We only allow replacement of a dummy stream.
                return;

            newStream.SetLength(_fileSizeInBytes);
            _stream = newStream;
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

        #endregion Common

        #region File transfer resuming

        private async void AppSuspendingHandler(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            if (!IsPlaceholder)
                await _fileTransferResumer.UpdateTransfer(_friendNumber, _fileNumber, TransferredBytes);

            deferral.Complete();
        }

        private async void FriendConnectionStatusChangedHandler(object sender,
            ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (_friendNumber != e.FriendNumber || IsPlaceholder)
                return;

            if (!_toxModel.IsFriendOnline(e.FriendNumber))
            {
                await _fileTransferResumer.UpdateTransfer(_friendNumber, _fileNumber, TransferredBytes);
                State = FileTransferState.Cancelled;
            }
        }

        #endregion File transfer resuming
    }
}