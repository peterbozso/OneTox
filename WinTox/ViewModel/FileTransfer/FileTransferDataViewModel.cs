using WinTox.Common;

namespace WinTox.ViewModel.FileTransfer
{
    public enum FileTransferDirection
    {
        Up,
        Down
    }

    public class FileTransferDataViewModel : ViewModelBase
    {
        private readonly FileTransfersViewModel _fileTransfers;
        private double _progress;
        private RelayCommand _removeTransferCommand;

        public FileTransferDataViewModel(FileTransfersViewModel fileTransfers, int fileNumber, string name,
            FileTransferDirection direction)
        {
            _fileTransfers = fileTransfers;
            FileNumber = fileNumber;
            Name = name;
            Direction = direction;
            Progress = 0;
        }

        public int FileNumber { get; private set; }
        public string Name { get; private set; }
        public FileTransferDirection Direction { get; private set; }

        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged();
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