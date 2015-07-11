using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using WinTox.Common;

namespace WinTox.ViewModel
{
    public class CallViewModel : ViewModelBase
    {
        private InMemoryRandomAccessStream _audioStream;
        private RelayCommand _changeMuteCommand;
        private bool _isDuringCall;
        private bool _isMuted;
        private MediaCapture _mediaCapture;
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

        public RelayCommand ChangeMuteCommand
        {
            get
            {
                return _changeMuteCommand ?? (_changeMuteCommand = new RelayCommand(() =>
                {
                    IsMuted = !IsMuted;
                    _mediaCapture.AudioDeviceController.Muted = IsMuted;
                }));
            }
        }

        #region Starting a call by the user

        public RelayCommand StartCallByUserCommand
        {
            get
            {
                return _startCallByUserCommand ??
                       (_startCallByUserCommand = new RelayCommand(async () =>
                       {
                           _mediaCapture = new MediaCapture();
                           var captureInitSettings = new MediaCaptureInitializationSettings
                           {
                               StreamingCaptureMode = StreamingCaptureMode.Audio,
                               MediaCategory = MediaCategory.Communications
                           };

                           var successfulInitialization =
                               await InitializeMediaCapture(_mediaCapture, captureInitSettings);
                           if (!successfulInitialization)
                               return;

                           _mediaCapture.Failed += MediaCaptureFailedHandler;
                           _mediaCapture.RecordLimitationExceeded += MediaCaptureRecordLimitationExceededHandler;

                           await StartRecording();

                           IsDuringCall = true;
                       }));
            }
        }

        public event EventHandler<string> StartCallByUserFailed;

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

        private void RaiseStartCallByUserFailed(string errorMessage)
        {
            if (StartCallByUserFailed != null)
                StartCallByUserFailed(this, errorMessage);
        }

        private async Task StartRecording()
        {
            _audioStream = new InMemoryRandomAccessStream();
            var encodingProfile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto);
            await _mediaCapture.StartRecordToStreamAsync(encodingProfile, _audioStream);
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

        #region Stopping a call by the user

        public RelayCommand StopCallByUserCommand
        {
            get
            {
                return _stopCallByUserCommand ??
                       (_stopCallByUserCommand = new RelayCommand(async () =>
                       {
                           await StopRecording();
                           IsDuringCall = false;
                       }));
            }
        }

        private async Task StopRecording()
        {
            await _mediaCapture.StopRecordAsync();
            _audioStream.Dispose();
        }

        #endregion
    }
}