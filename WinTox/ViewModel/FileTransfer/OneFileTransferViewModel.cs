using WinTox.Common;

namespace WinTox.ViewModel.FileTransfer
{
    public enum FileTransferState
    {
        Uploading,
        Downloading,
        Finished,
        Cancelled
    }

    public class OneFileTransferViewModel : ViewModelBase
    {
        private readonly FileTransfersViewModel _fileTransfers;
        private RelayCommand _cancelTransferByUserCommand;
        private bool _isNotPlaceholder;
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

                if (_progress.Equals(100.0))
                {
                    State = FileTransferState.Finished;
                    FileNumber = -1;
                }
            }
        }

        public RelayCommand CancelTransferByUserCommand
        {
            get
            {
                return _cancelTransferByUserCommand ?? (_cancelTransferByUserCommand = new RelayCommand(
                    () => { _fileTransfers.CancelTransfer(FileNumber); }));
            }
        }

        public void CancelTransferByFriend()
        {
            State = FileTransferState.Cancelled;
            FileNumber = -1;
            Progress = 0.0;
        }
    }
}