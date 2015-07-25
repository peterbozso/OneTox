using System;
using Windows.Storage;
using Windows.Storage.Pickers;
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
            IsPlaceholder = false;
        }

        #region Fields

        private readonly FileTransfersViewModel _fileTransfers;
        private FileTransferState _state;
        private bool _isPlaceholder;
        private double _progress;
        private readonly TransferDirection _direction;
        private RelayCommand _acceptTransferByUserCommand;
        private RelayCommand _cancelTransferByUserCommand;
        private RelayCommand _pauseTransferByUserCommand;
        private RelayCommand _resumeTransferByUserCommand;

        #endregion

        #region Properties

        public int FileNumber { get; private set; }

        public string Name { get; private set; }

        public FileTransferState State
        {
            get { return _state; }
            private set
            {
                if (value == _state)
                    return;
                _state = value;
                RaisePropertyChanged();

                IsPlaceholder = value == FileTransferState.Finished || value == FileTransferState.Cancelled;
            }
        }

        public bool IsPlaceholder
        {
            get { return _isPlaceholder; }
            private set
            {
                if (value == _isPlaceholder)
                    return;
                _isPlaceholder = value;
                RaisePropertyChanged();
            }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (value.Equals(_progress))
                    return;
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

        public RelayCommand AcceptTransferByUserCommand
        {
            get
            {
                return _acceptTransferByUserCommand ?? (_acceptTransferByUserCommand = new RelayCommand(async () =>
                {
                    var folderPicker = new FolderPicker();
                    folderPicker.FileTypeFilter.Add("*");
                    var saveFolder = await folderPicker.PickSingleFolderAsync();
                    if (saveFolder == null)
                        return;

                    var saveFile = await saveFolder.CreateFileAsync(Name, CreationCollisionOption.GenerateUniqueName);
                    await _fileTransfers.AcceptTransferByUser(FileNumber, saveFile);

                    State = FileTransferState.Downloading;
                }));
            }
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