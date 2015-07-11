using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
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

        #region Starting a call by the user

        public RelayCommand StartCallByUserCommand
        {
            get
            {
                return _startCallByUserCommand ??
                       (_startCallByUserCommand = new RelayCommand(async () =>
                       {
                           var mediaCapture = new MediaCapture();
                           var captureInitSettings = new MediaCaptureInitializationSettings
                           {
                               StreamingCaptureMode = StreamingCaptureMode.Audio
                           };

                           var successfulInitialization =
                               await InitializeMediaCapture(mediaCapture, captureInitSettings);
                           if (!successfulInitialization)
                               return;

                           mediaCapture.Failed += MediaCaptureFailedHandler;
                           mediaCapture.RecordLimitationExceeded += MediaCaptureRecordLimitationExceededHandler;

                           IsDuringCall = true;
                       }));
            }
        }

        private async Task<bool> InitializeMediaCapture(MediaCapture mediaCapture,
            MediaCaptureInitializationSettings captureInitSettings)
        {
            // Error messages from: https://msdn.microsoft.com/en-us/library/windows/apps/hh768223.aspx#additional_usage_guidance
            try
            {
                await mediaCapture.InitializeAsync(captureInitSettings);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                RaiseStartCallByUserFailed(
                    "Your microphone is currently turned off. To change your microphone setting, open the settings charm and tap permissions. " +
                    "Then tap this button to start using microphone again.");
                return false;
            }
            catch (Exception)
            {
                RaiseStartCallByUserFailed(
                    "You do not have the required microphone present on your system.");
                return false;
            }
        }

        public event EventHandler<string> StartCallByUserFailed;

        private void RaiseStartCallByUserFailed(string errorMessage)
        {
            if (StartCallByUserFailed != null)
                StartCallByUserFailed(this, errorMessage);
        }

        private void MediaCaptureRecordLimitationExceededHandler(MediaCapture sender)
        {
            throw new NotImplementedException();
        }

        private void MediaCaptureFailedHandler(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}