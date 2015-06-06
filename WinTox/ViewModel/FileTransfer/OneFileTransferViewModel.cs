using WinTox.Common;

namespace WinTox.ViewModel.FileTransfer
{
    public enum FileTransferState
    {
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
        private FileTransferState _beforePause; // TODO: Maybe refactor it out later!
        private RelayCommand _cancelTransferByUserCommand;
        private bool _isNotPlaceholder;
        private RelayCommand _pauseResumeTransferByUserCommand;
        private double _progress;
        private FileTransferState _state;

        public OneFileTransferViewModel(FileTransfersViewModel fileTransfers, int fileNumber, string name,
            FileTransferState state)
        {
            _fileTransfers = fileTransfers;
            FileNumber = fileNumber;
            Name = name;
            State = state;
            Progress = 0;
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

        public RelayCommand PauseResumeTransferByUserCommand
        {
            get
            {
                return _pauseResumeTransferByUserCommand ?? (_pauseResumeTransferByUserCommand = new RelayCommand(
                    () =>
                    {
                        if (State == FileTransferState.Downloading || State == FileTransferState.Uploading)
                        {
                            _beforePause = State;
                            State = FileTransferState.PausedByUser;
                            _fileTransfers.PauseTransferByUser(FileNumber);
                        }
                        else if (State == FileTransferState.PausedByUser)
                        {
                            State = _beforePause;
                            _fileTransfers.ResumeTransferByUser(FileNumber);
                        }
                    }));
            }
        }

        public void PauseTransferByFriend()
        {
            _beforePause = State;
            State = FileTransferState.PausedByFriend;
        }

        public void ResumeTransferByFriend()
        {
            State = _beforePause;
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