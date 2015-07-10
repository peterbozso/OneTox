using WinTox.Common;

namespace WinTox.ViewModel
{
    public class CallViewModel : ViewModelBase
    {
        private RelayCommand _changeMuteCommand;
        private bool _isDuringCall;
        private bool _isMuted;
        private RelayCommand _startCallByUserCommand;
        private RelayCommand _stopCallByUserCommand;

        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                _isMuted = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDuringCall
        {
            get { return _isDuringCall; }
            private set
            {
                _isDuringCall = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand StartCallByUserCommand
        {
            get
            {
                return _startCallByUserCommand ??
                       (_startCallByUserCommand = new RelayCommand(() => { IsDuringCall = true; }));
            }
        }

        public RelayCommand StopCallByUserCommand
        {
            get
            {
                return _stopCallByUserCommand ??
                       (_stopCallByUserCommand = new RelayCommand(() => { IsDuringCall = false; }));
            }
        }

        public RelayCommand ChangeMuteCommand
        {
            get { return _changeMuteCommand ?? (_changeMuteCommand = new RelayCommand(() => { IsMuted = !IsMuted; })); }
        }
    }
}