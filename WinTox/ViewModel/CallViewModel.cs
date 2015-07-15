using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Capture;
using NAudio.Wave;
using SharpTox.Av;
using WinTox.Common;
using WinTox.Model;

namespace WinTox.ViewModel
{
    public class CallViewModel : ViewModelBase
    {
        private readonly int _friendNumber;
        private bool _canSend;
        private RelayCommand _changeMuteCommand;
        private bool _isDuringCall;
        private bool _isMuted;
        private IWaveIn _recorder;
        private List<short> _sendBuffer;
        private RelayCommand _startCallByUserCommand;
        private RelayCommand _stopCallByUserCommand;

        public CallViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            ToxAvModel.Instance.CallStateChanged += CallStateChangedHandler;
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
                    if (IsMuted)
                    {
                        _recorder.StopRecording();
                    }
                    else
                    {
                        _recorder.StartRecording();
                    }
                }));
            }
        }

        private void CallStateChangedHandler(object sender, ToxAvEventArgs.CallStateEventArgs e)
        {
            if (e.FriendNumber != _friendNumber)
                return;

            if (e.State.HasFlag(ToxAvFriendCallState.ReceivingAudio))
            {
                _canSend = true;
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
                           var microphoneIsAvailabe = await IsMicrophoneAvailable();
                           if (!microphoneIsAvailabe)
                               return;

                           StartRecording();

                           IsMuted = false;

                           var successfulCall = ToxAvModel.Instance.Call(_friendNumber, 48, 0);
                           Debug.WriteLine("Calling " + _friendNumber + " " + successfulCall);

                           IsDuringCall = true;
                       }));
            }
        }

        private async Task<bool> IsMicrophoneAvailable()
        {
            // Error messages from: https://msdn.microsoft.com/en-us/library/windows/apps/hh768223.aspx#additional_usage_guidance
            try
            {
                var mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();
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

        private void StartRecording()
        {
            _sendBuffer = new List<short>();

            _recorder = new WasapiCaptureRT
            {
                WaveFormat = new WaveFormat(48000, 16, 1)
            };
            _recorder.DataAvailable += DataAvailableHandler;

            _recorder.StartRecording();
        }

        private void DataAvailableHandler(object sender, WaveInEventArgs e)
        {
            if (!_canSend)
                return;

            // It doesn't make much sense, but WaveInEventArgs.Buffer.Length != WaveInEventArgs.BytesRecorded.
            // Let's just call that a feature of NAudio... ;)
            var shorts = new short[e.BytesRecorded/2];
            Buffer.BlockCopy(e.Buffer, 0, shorts, 0, e.BytesRecorded);

            _sendBuffer.AddRange(shorts);

            if (_sendBuffer.Count == 960)
            {
                ToxAvModel.Instance.SendAudioFrame(_friendNumber, new ToxAvAudioFrame(_sendBuffer.ToArray(), 48000, 1));
                _sendBuffer.Clear();
            }
        }

        public event EventHandler<string> StartCallByUserFailed;

        #endregion

        #region Stopping a call by the user

        public RelayCommand StopCallByUserCommand
        {
            get
            {
                return _stopCallByUserCommand ??
                       (_stopCallByUserCommand = new RelayCommand(async () =>
                       {
                           StopRecording();
                           ToxAvModel.Instance.SendControl(_friendNumber, ToxAvCallControl.Cancel);
                           IsDuringCall = false;
                       }));
            }
        }

        private void StopRecording()
        {
            _recorder.StopRecording();
            _recorder.Dispose();
        }

        #endregion
    }
}