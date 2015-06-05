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
        private double _progress;
        private FileTransferState _state;
        private RelayCommand _removeTransferCommand;

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
            }
        }

        public bool IsActive
        {
            get { return State == FileTransferState.Downloading || State == FileTransferState.Uploading; }
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

        public RelayCommand CancelTransferCommand
        {
            get
            {
                return _removeTransferCommand ?? (_removeTransferCommand = new RelayCommand(
                    () => { _fileTransfers.CancelTransfer(FileNumber); }));
            }
        }
    }
}