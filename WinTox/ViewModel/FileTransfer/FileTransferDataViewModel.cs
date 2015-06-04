namespace WinTox.ViewModel.FileTransfer
{
    public enum FileTransferDirection
    {
        Up,
        Down
    }

    public class FileTransferDataViewModel : ViewModelBase
    {
        private double _progress;

        public FileTransferDataViewModel(int fileNumber, string name, FileTransferDirection direction)
        {
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
    }
}