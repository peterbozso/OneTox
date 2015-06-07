using WinTox.Common;

namespace WinTox.ViewModel.FileTransfer
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
        private readonly FileTransfersViewModel _fileTransfers;
        private RelayCommand _cancelTransferByUserCommand;
        private bool _isNotPlaceholder;
        private FileTransferState _lastState;
        private RelayCommand _pauseTransferByUserCommand;
        private double _progress;
        private RelayCommand _rasumeTransferByUserCommand;
        private FileTransferState _state;

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

        public RelayCommand CancelTransferByUserCommand
        {
            get
            {
                return _cancelTransferByUserCommand ?? (_cancelTransferByUserCommand = new RelayCommand(
                    () => { _fileTransfers.CancelTransferByUser(FileNumber); }));
            }
        }

        public RelayCommand PauseTransferByUserCommand
        {
            get
            {
                return _pauseTransferByUserCommand ?? (_pauseTransferByUserCommand = new RelayCommand(
                    () =>
                    {
                        if (State == FileTransferState.Downloading || State == FileTransferState.Uploading)
                        {
                            _lastState = State;
                            State = FileTransferState.PausedByUser;
                            _fileTransfers.PauseTransferByUser(FileNumber);
                        }
                    }));
            }
        }

        public RelayCommand ResumeTransferByUserCommand
        {
            get
            {
                return _rasumeTransferByUserCommand ?? (_rasumeTransferByUserCommand = new RelayCommand(
                    () =>
                    {
                        if (State == FileTransferState.PausedByUser)
                        {
                            State = _lastState;
                            _fileTransfers.ResumeTransferByUser(FileNumber);
                        }
                    }));
            }
        }

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
    }
}