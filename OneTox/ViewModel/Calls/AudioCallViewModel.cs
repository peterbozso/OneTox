using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Capture;
using Windows.UI.Core;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Win8.Wave.WaveOutputs;
using OneTox.Common;
using OneTox.Helpers;
using OneTox.Model;
using SharpTox.Av;

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

        public AudioCallViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            ToxAvModel.Instance.CallStateChanged += CallStateChangedHandler;
            ToxAvModel.Instance.AudioFrameReceived += AudioFrameReceivedHandler;
            ToxAvModel.Instance.CallRequestReceived += CallRequestReceivedHandler;
            SetAudioValues();
        }

        private void SetAudioValues()
        {
            // TODO: Set these based on app settings!
            _bitRate = 48;
            _samplingRate = _bitRate*1000;
            _audioFrameSize = _samplingRate*KAudioLength/1000;
        }

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

        #region Fields

        private const int KAudioLength = 20; // Based on measurements. Take it with a grain of salt!
        private const string KRingInFileName = "ring-in.wav";
        private const string KRingOutFileName = "ring-out.wav";
        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        private readonly int _friendNumber;
        private RelayCommand _acceptCallCommand;
        private int _audioFrameSize;
        private int _bitRate;
        private RelayCommand _cancelCallCommand;
        private RelayCommand _changeMuteCommand;
        private bool _isMuted;
        private IWavePlayer _player;
        private IWaveIn _recorder;
        private int _samplingRate;
        private List<short> _sendBuffer;
        private RelayCommand _startCallCommand;
        private CallState _state;
        private RelayCommand _stopCallCommand;
        private BufferedWaveProvider _waveProvider;

        #endregion

        #region Ringing events

        public event EventHandler<string> StartRinging;
        public event EventHandler StopRinging;

        private void RaiseStartRinging(string ringFileName)
        {
            if (StartRinging != null)
                StartRinging(this, ringFileName);
        }

        private void RaiseStopRinging()
        {
            if (StopRinging != null)
                StopRinging(this, EventArgs.Empty);
        }

        #endregion

        #region Commands

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
                        await TryStartRecording();
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

        public RelayCommand AcceptCallCommand
        {
            get
            {
                return _acceptCallCommand ??
                       (_acceptCallCommand =
                           new RelayCommand(async () =>
                           {
                               ToxAvModel.Instance.Answer(_friendNumber, _bitRate, 0);
                               await TryStartRecording();
                               TryStartPlaying();
                               State = CallState.DuringCall;
                           }));
            }
        }

        public RelayCommand DeclineCallCommand
        {
            get
            {
                return _cancelCallCommand ?? (_cancelCallCommand = new RelayCommand(() =>
                {
                    ToxAvModel.Instance.SendControl(_friendNumber, ToxAvCallControl.Cancel);
                    State = CallState.Default;
                }));
            }
        }

        public RelayCommand StartCallCommand
        {
            get
            {
                return _startCallCommand ??
                       (_startCallCommand = new RelayCommand(() =>
                       {
                           ToxAvModel.Instance.Call(_friendNumber, _bitRate, 0);
                           IsMuted = false;
                           State = CallState.OutgoingCall;
                       }));
            }
        }

        public RelayCommand StopCallCommand
        {
            get
            {
                return _stopCallCommand ??
                       (_stopCallCommand = new RelayCommand(() =>
                       {
                           StopRecording();
                           StopPlaying();
                           ToxAvModel.Instance.SendControl(_friendNumber, ToxAvCallControl.Cancel);
                           State = CallState.Default;
                       }));
            }
        }

        #endregion

        #region Events handlers

        private async void CallStateChangedHandler(object sender, ToxAvEventArgs.CallStateEventArgs e)
        {
            if (e.FriendNumber != _friendNumber)
                return;

            if (e.State.HasFlag(ToxAvFriendCallState.ReceivingAudio) ||
                e.State.HasFlag(ToxAvFriendCallState.SendingAudio))
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { State = CallState.DuringCall; });
            }

            if (e.State.HasFlag(ToxAvFriendCallState.ReceivingAudio))
            {
                await TryStartRecording();
            }

            if (e.State.HasFlag(ToxAvFriendCallState.SendingAudio))
            {
                TryStartPlaying();
            }

            if (e.State.HasFlag(ToxAvFriendCallState.Finished) || e.State.HasFlag(ToxAvFriendCallState.Error))
            {
                StopRecording();
                StopPlaying();
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { State = CallState.Default; });
            }
        }

        private async void CallRequestReceivedHandler(object sender, ToxAvEventArgs.CallRequestEventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { State = CallState.IncomingCall; });
        }

        #endregion

        #region Audio sending

        private async Task TryStartRecording()
        {
            if (_recorder == null)
            {
                var microphoneIsAvailabe = await IsMicrophoneAvailable();
                if (microphoneIsAvailabe)
                {
                    _sendBuffer = new List<short>();

                    _recorder = new WasapiCaptureRT
                    {
                        WaveFormat = new WaveFormat(_samplingRate, 16, 1)
                    };
                    _recorder.DataAvailable += DataAvailableHandler;

                    _recorder.StartRecording();

                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => { IsMuted = false; });
                }
                else
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => { IsMuted = true; });
                }
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
                RaiseMicrophoneIsNotAvailable(
                    "Your microphone is currently turned off. To change your microphone setting, open the settings charm and tap permissions. " +
                    "Then tap the mute button to start using microphone again.");
                return false;
            }
            catch (Exception)
            {
                RaiseMicrophoneIsNotAvailable(
                    "You do not have the required microphone present on your system.");
                return false;
            }
        }

        private async void RaiseMicrophoneIsNotAvailable(string errorMessage)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (MicrophoneIsNotAvailable != null)
                    MicrophoneIsNotAvailable(this, errorMessage);
            });
        }

        public event EventHandler<string> MicrophoneIsNotAvailable;

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

        private void TryStartPlaying()
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
            if (_waveProvider == null)
            {
                // TODO: Replace it with actual values received from friend!
                _waveProvider = new BufferedWaveProvider(new WaveFormat(_samplingRate, 16, 1));
                _waveProvider.DiscardOnBufferOverflow = true;
            }

            return _waveProvider;
        }

        private void AudioFrameReceivedHandler(object sender, ToxAvEventArgs.AudioFrameEventArgs e)
        {
            if (_player == null || _waveProvider == null)
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