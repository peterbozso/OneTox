using System;
using OneTox.Common;
using OneTox.Helpers;

namespace OneTox.ViewModel.Calls
{
    public class AudioCallViewModel : ObservableObject
    {
        public enum CallState
        {
            Default,
            DuringCall,
            OutgoingCall,
            IncomingCall
        }

        private int _friendNumber;

        public AudioCallViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
        }

        public event EventHandler<string> MicrophoneIsNotAvailable;

        #region Constants

        private const string KRingInFileName = "ring-in.wav";
        private const string KRingOutFileName = "ring-out.wav";

        #endregion

        #region Fields

        private bool _isMuted;
        private CallState _state;
        private RelayCommand _cancelCallCommand;
        private RelayCommand _changeMuteCommand;
        private RelayCommand _stopCallCommand;
        private RelayCommand _startCallCommand;
        private RelayCommand _acceptCallCommand;

        #endregion

        #region Properties

        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                if (value == _isMuted)
                    return;
                _isMuted = value;
                RaisePropertyChanged();
            }
        }

        public CallState State
        {
            get { return _state; }
            set
            {
                if (value == _state)
                    return;

                switch (value)
                {
                    case CallState.Default:
                        RaiseStopRinging();
                        break;
                    case CallState.DuringCall:
                        RaiseStopRinging();
                        break;
                    case CallState.IncomingCall:
                        RaiseStartRinging(KRingInFileName);
                        break;
                    case CallState.OutgoingCall:
                        RaiseStartRinging(KRingOutFileName);
                        break;
                }

                _state = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Ringing events

        public event EventHandler<string> StartRinging;
        public event EventHandler StopRinging;

        private void RaiseStartRinging(string ringFileName)
        {
            StartRinging?.Invoke(this, ringFileName);
        }

        private void RaiseStopRinging()
        {
            StopRinging?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Commands

        public RelayCommand ChangeMuteCommand
        {
            get { return _changeMuteCommand ?? (_changeMuteCommand = new RelayCommand(() => { })); }
        }

        public RelayCommand AcceptCallCommand
        {
            get
            {
                return _acceptCallCommand ??
                       (_acceptCallCommand =
                           new RelayCommand(() => { }));
            }
        }

        public RelayCommand DeclineCallCommand
        {
            get { return _cancelCallCommand ?? (_cancelCallCommand = new RelayCommand(() => { })); }
        }

        public RelayCommand StartCallCommand
        {
            get
            {
                return _startCallCommand ??
                       (_startCallCommand = new RelayCommand(() => { }));
            }
        }

        public RelayCommand StopCallCommand
        {
            get
            {
                return _stopCallCommand ??
                       (_stopCallCommand = new RelayCommand(() => { }));
            }
        }

        #endregion
    }
}