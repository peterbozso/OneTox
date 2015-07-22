using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Capture;
using Windows.UI.Core;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Win8.Wave.WaveOutputs;
using SharpTox.Av;
using WinTox.Common;
using WinTox.Model;

namespace WinTox.ViewModel
{
    public enum CallState
    {
        Default,
        DuringCall,
        Calling
    }

    public class CallViewModel : ViewModelBase
    {
        private const int KAudioLength = 20; // Based on measurements. Take it with a grain of salt!
        private readonly int _friendNumber;
        private int _audioFrameSize;
        private int _bitRate;
        private RelayCommand _changeMuteCommand;
        private bool _isMuted;
        private IWavePlayer _player;
        private IWaveIn _recorder;
        private int _samplingRate;
        private List<short> _sendBuffer;
        private RelayCommand _startCallByUserCommand;
        private CallState _state;
        private RelayCommand _stopCallByUserCommand;
        private BufferedWaveProvider _waveProvider;

        public CallViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            ToxAvModel.Instance.CallStateChanged += CallStateChangedHandler;
            ToxAvModel.Instance.AudioFrameReceived += AudioFrameReceivedHandler;
            SetAudioValues();
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

        public CallState State
        {
            get { return _state; }
            set
            {
                _state = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand ChangeMuteCommand
        {
            get
            {
                return _changeMuteCommand ?? (_changeMuteCommand = new RelayCommand(async () =>
                {
                    if (_recorder == null)
                        // This means that we weren't able to instantiate it due to missing microphone or access permission.
                    {
                        // So we give it another go, maybe the user enabled the microphone/plugged one in since then.
                        var microphoneIsAvailabe = await IsMicrophoneAvailable();
                        if (microphoneIsAvailabe)
                        {
                            StartRecording();
                            IsMuted = false;
                        }

                        return;
                    }

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

        private void SetAudioValues()
        {
            // TODO: Set these based on app settings!
            _bitRate = 48;
            _samplingRate = _bitRate*1000;
            _audioFrameSize = _samplingRate*KAudioLength/1000;
        }

        private async void CallStateChangedHandler(object sender, ToxAvEventArgs.CallStateEventArgs e)
        {
            if (e.FriendNumber != _friendNumber)
                return;

            if (e.State.HasFlag(ToxAvFriendCallState.ReceivingAudio) ||
                e.State.HasFlag(ToxAvFriendCallState.SendingAudio))
            {
                await
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => { State = CallState.DuringCall; });
            }

            if (e.State.HasFlag(ToxAvFriendCallState.ReceivingAudio))
            {
                var microphoneIsAvailabe = await IsMicrophoneAvailable();
                if (!microphoneIsAvailabe)
                {
                    IsMuted = true;
                }
                else
                {
                    StartRecording();
                }
            }

            if (e.State.HasFlag(ToxAvFriendCallState.SendingAudio))
            {
                TrySetupAudioReceiving();
            }
        }

        #region Audio sending

        private void DataAvailableHandler(object sender, WaveInEventArgs e)
        {
            // It doesn't make much sense, but WaveInEventArgs.Buffer.Length != WaveInEventArgs.BytesRecorded.
            // Let's just call that a feature of NAudio... ;)
            var shorts = new short[e.BytesRecorded/2];
            Buffer.BlockCopy(e.Buffer, 0, shorts, 0, e.BytesRecorded);

            _sendBuffer.AddRange(shorts);

            if (_sendBuffer.Count == _audioFrameSize)
            {
                ToxAvModel.Instance.SendAudioFrame(_friendNumber,
                    new ToxAvAudioFrame(_sendBuffer.ToArray(), _samplingRate, 1));
                _sendBuffer.Clear();
            }
        }

        #endregion

        #region Starting a call by the user

        public RelayCommand StartCallByUserCommand
        {
            get
            {
                return _startCallByUserCommand ??
                       (_startCallByUserCommand = new RelayCommand(async () =>
                       {
                           var successfulCall = ToxAvModel.Instance.Call(_friendNumber, _bitRate, 0);
                           Debug.WriteLine("Calling " + _friendNumber + " " + successfulCall);

                           IsMuted = false;
                           State = CallState.Calling;
                       }));
            }
        }

        private async Task<bool> IsMicrophoneAvailable()
        {
            // Error messages from: https://msdn.microsoft.com/en-us/library/windows/apps/hh768223.aspx#additional_usage_guidance
            try
            {
                // We do this initialization (and tear down immediately after) just to check if we have access to a microphone.
                // We'll do the actual work with NAudio, since MediaCapture doesn't supply raw PCM data that we need.
                var mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();
                mediaCapture.Dispose();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                RaiseStartCallByUserFailed(
                    "Your microphone is currently turned off. To change your microphone setting, open the settings charm and tap permissions. " +
                    "Then tap the mute button to start using microphone again.");
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
                WaveFormat = new WaveFormat(_samplingRate, 16, 1)
            };
            _recorder.DataAvailable += DataAvailableHandler;

            _recorder.StartRecording();
        }

        public event EventHandler<string> StartCallByUserFailed;

        #endregion

        #region Stopping a call by the user

        public RelayCommand StopCallByUserCommand
        {
            get
            {
                return _stopCallByUserCommand ??
                       (_stopCallByUserCommand = new RelayCommand(() =>
                       {
                           StopRecording();
                           StopPlaying();
                           ToxAvModel.Instance.SendControl(_friendNumber, ToxAvCallControl.Cancel);
                           State = CallState.Default;
                       }));
            }
        }

        private void StopRecording()
        {
            if (_recorder == null)
                return;

            _recorder.StopRecording();
            _recorder.Dispose();
            _recorder = null;
        }

        #endregion

        #region Audio receiving

        private void TrySetupAudioReceiving()
        {
            if (_player == null)
            {
                _player = new WasapiOutRT(AudioClientShareMode.Shared, 0);
                _player.Init(CreateReader);
                _player.Play();
            }
        }

        private IWaveProvider CreateReader()
        {
            return _waveProvider ?? (_waveProvider = new BufferedWaveProvider(new WaveFormat(_samplingRate, 16, 1)));
            // TODO: Replace it with actual values received from friend!
        }

        private void AudioFrameReceivedHandler(object sender, ToxAvEventArgs.AudioFrameEventArgs e)
        {
            if (_player == null)
                return;

            var bytes = new byte[e.Frame.Data.Length*2];
            Buffer.BlockCopy(e.Frame.Data, 0, bytes, 0, e.Frame.Data.Length);

            _waveProvider.AddSamples(bytes, 0, bytes.Length);
        }

        private void StopPlaying()
        {
            if (_player == null)
                return;

            _player.Stop();
            _player.Dispose();
            _player = null;
        }

        #endregion
    }
}