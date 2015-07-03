using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using WinTox.Common;
using WinTox.Model;

namespace WinTox.ViewModel.FileTransfers
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

    public class OneFileTransferViewModel : ViewModelBase
    {
        public OneFileTransferViewModel(FileTransfersViewModel fileTransfers, int fileNumber, string name,
            TransferDirection direction)
        {
            _fileTransfers = fileTransfers;
            FileNumber = fileNumber;
            Name = name;
            Progress = 0;
            _direction = direction;
            SetInitialStateBasedOnDirection();
        }

        #region Fields

        private readonly FileTransfersViewModel _fileTransfers;
        private FileTransferState _state;
        private bool _isNotPlaceholder;
        private double _progress;
        private readonly TransferDirection _direction;
        private RelayCommand _pauseTransferByUserCommand;
        private RelayCommand _resumeTransferByUserCommand;
        private RelayCommand _cancelTransferByUserCommand;

        #endregion

        #region Properties

        public int FileNumber { get; private set; }

        public string Name { get; private set; }

        public FileTransferState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                RaisePropertyChanged();
                IsNotPlaceholder = _state != FileTransferState.Finished && _state != FileTransferState.Cancelled;
            }
        }

        public bool IsNotPlaceholder
        {
            get { return _isNotPlaceholder; }
            set
            {
                _isNotPlaceholder = value;
                RaisePropertyChanged();
            }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Helper methods

        private void SetInitialStateBasedOnDirection()
        {
            switch (_direction)
            {
                case TransferDirection.Up:
                    State = FileTransferState.BeforeUpload;
                    break;
                case TransferDirection.Down:
                    State = FileTransferState.BeforeDownload;
                    break;
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

        #endregion

        #region Changes coming from the View, being relayed to the Model

        public async Task AcceptTransferByUser(StorageFile saveFile)
        {
            State = FileTransferState.Downloading;
            await _fileTransfers.AcceptTransferByUser(FileNumber, saveFile);
        }

        public RelayCommand CancelTransferByUserCommand
        {
            get
            {
                return _cancelTransferByUserCommand ?? (_cancelTransferByUserCommand = new RelayCommand(
                    async () => { await _fileTransfers.CancelTransferByUser(this); }));
            }
        }

        public RelayCommand PauseTransferByUserCommand
        {
            get
            {
                return _pauseTransferByUserCommand ?? (_pauseTransferByUserCommand = new RelayCommand(
                    async () =>
                    {
                        if (State == FileTransferState.Downloading || State == FileTransferState.Uploading)
                        {
                            State = FileTransferState.PausedByUser;
                            await _fileTransfers.PauseTransferByUser(FileNumber);
                        }
                    }));
            }
        }

        public RelayCommand ResumeTransferByUserCommand
        {
            get
            {
                return _resumeTransferByUserCommand ?? (_resumeTransferByUserCommand = new RelayCommand(
                    async () =>
                    {
                        if (State == FileTransferState.PausedByUser)
                        {
                            SetResumingStateBasedOnDirection();
                            await _fileTransfers.ResumeTransferByUser(FileNumber);
                        }
                    }));
            }
        }

        #endregion

        #region Changes coming from the Model, being relayed to the View

        public void FinishTransfer()
        {
            State = FileTransferState.Finished;
            Progress = 100.0;
        }

        public void CancelTransferByFriend()
        {
            State = FileTransferState.Cancelled;
            Progress = 100.0;
        }

        public void PauseTransferByFriend()
        {
            if (State != FileTransferState.Uploading && State != FileTransferState.Downloading)
                return;

            State = FileTransferState.PausedByFriend;
        }

        public void ResumeTransferByFriend()
        {
            switch (State)
            {
                case FileTransferState.BeforeUpload:
                    State = FileTransferState.Uploading;
                    break;
                case FileTransferState.BeforeDownload:
                    State = FileTransferState.Downloading;
                    break;
            }

            if (State != FileTransferState.PausedByFriend)
                return;

            SetResumingStateBasedOnDirection();
        }

        #endregion
    }
}