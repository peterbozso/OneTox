namespace WinTox.ViewModel
{
    public class CallViewModel : ViewModelBase
    {
        private bool _isMuted;

        public CallViewModel()
        {
            IsMuted = false;
        }

        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                _isMuted = value;
                RaisePropertyChanged();
            }
        }
    }
}