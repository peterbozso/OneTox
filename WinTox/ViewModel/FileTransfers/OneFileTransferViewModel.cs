using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using WinTox.Common;

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
            FileTransferState state)
        {
            _fileTransfers = fileTransfers;
            FileNumber = fileNumber;
            Name = name;
            State = state;
            Progress = 0;

            _lastState = state;
            switch (state)
            {
                case FileTransferState.Uploading:
                    State = FileTransferState.BeforeUpload;
                    break;
                case FileTransferState.Downloading:
                    State = FileTransferState.BeforeDownload;
                    break;
            }
        }

        #region Fields

        private readonly FileTransfersViewModel _fileTransfers;
        private FileTransferState _state;
        private bool _isNotPlaceholder;
        private double _progress;
        private FileTransferState _lastState;
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

        #region Actions coming from the View

        public async Task AcceptTransferByUser(StorageFile saveFile)
        {
            State = FileTransferState.Downloading;
            var saveStream = (await saveFile.OpenAsync(FileAccessMode.ReadWrite)).AsStream();
            await _fileTransfers.AcceptTransferByUser(FileNumber, saveStream);
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
                            _lastState = State;
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
                            State = _lastState;
                            await _fileTransfers.ResumeTransferByUser(FileNumber);
                        }
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

        #endregion

        #region Actions coming from the Model

        public void PauseTransferByFriend()
        {
            if (State != FileTransferState.Uploading && State != FileTransferState.Downloading)
                return;

            _lastState = State;
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

            State = _lastState;
        }

        public void CancelTransferByFriend()
        {
            State = FileTransferState.Cancelled;
            Progress = 100.0;
        }

        public void FinishTransfer()
        {
            State = FileTransferState.Finished;
            Progress = 100.0;
        }

        #endregion
    }
}