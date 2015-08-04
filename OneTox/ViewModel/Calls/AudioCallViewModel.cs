using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.UI.Core;
using OneTox.Common;
using OneTox.Helpers;
using OneTox.Model;
using SharpTox.Av;

namespace OneTox.ViewModel.Calls
{
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

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

            InitializeRates();

            ToxAvModel.Instance.CallStateChanged += CallStateChangedHandler;
        }

        #region ToxAv event handlers

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

            _friendIsReceivingAudio = e.State.HasFlag(ToxAvFriendCallState.ReceivingAudio);

            if (e.State.HasFlag(ToxAvFriendCallState.Finished) || e.State.HasFlag(ToxAvFriendCallState.Error))
            {
                StopAudioGraph();
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { State = CallState.Default; });
            }
        }

        #endregion

        private void InitializeRates()
        {
            _bitRate = 48;
            _samplingRate = 48*1000;
            _frameSize = _samplingRate*KQuantumSize/1000;
        }

        private void RaiseMicrophoneIsNotAvailable(string errorMessage)
        {
            MicrophoneIsNotAvailable?.Invoke(this, errorMessage);
        }

        public event EventHandler<string> MicrophoneIsNotAvailable;

        #region Fields

        private readonly int _friendNumber;
        private AudioGraph _audioGraph;
        private AudioDeviceInputNode _microphoneInputNode;
        private AudioFrameOutputNode _toxOutputNode;
        private int _samplingRate;
        private int _bitRate;
        private int _frameSize;
        private bool _friendIsReceivingAudio;

        #endregion

        #region Audio sending

        private async Task StartAudioGraph()
        {
            await InitAudioGraph();

            var success = await CreateMicrophoneInputNode();
            if (!success)
            {
                IsMuted = true;
                return;
            }

            CreateToxOutputNode();
            _audioGraph.Start();
        }

        private void StopAudioGraph()
        {
            _audioGraph?.Stop();
        }

        private async Task InitAudioGraph()
        {
            var encodingProperties = AudioEncodingProperties.CreatePcm((uint) _samplingRate, 1, 16);

            // Don't modify DesiredSamplesPerQuantum! If you do, change KQuantumSize accordingly!
            var settings = new AudioGraphSettings(AudioRenderCategory.Communications)
            {
                EncodingProperties = encodingProperties
            };

            var result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                throw new Exception(result.Status.ToString());
            }

            _audioGraph = result.Graph;
        }

        private async Task<bool> CreateMicrophoneInputNode()
        {
            // Create a device output node
            var result = await _audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Communications);

            if (result.Status != AudioDeviceNodeCreationStatus.Success)
            {
                switch (result.Status)
                {
                    case AudioDeviceNodeCreationStatus.DeviceNotAvailable:
                        RaiseMicrophoneIsNotAvailable("You do not have the required microphone present on your system.");
                        return false;
                    case AudioDeviceNodeCreationStatus.AccessDenied:
                        RaiseMicrophoneIsNotAvailable(
                            "OneTox doesn't have permission to use your microphone. To change this, please go to the Settings app's Privacy section. " +
                            "Then click or tap the mute button to start using the microphone again.");
                        return false;
                    default:
                        throw new Exception(result.Status.ToString());
                }
            }

            _microphoneInputNode = result.DeviceInputNode;
            return true;
        }

        private void CreateToxOutputNode()
        {
            _toxOutputNode = _audioGraph.CreateFrameOutputNode();
            _audioGraph.QuantumProcessed += AudioGraphQuantumProcessedHandler;
            _microphoneInputNode.AddOutgoingConnection(_toxOutputNode);
        }

        private void AudioGraphQuantumProcessedHandler(AudioGraph sender, object args)
        {
            if (!_friendIsReceivingAudio)
                return;

            var frame = _toxOutputNode.GetFrame();
            ProcessFrameOutput(frame);
        }

        private unsafe void ProcessFrameOutput(AudioFrame frame)
        {
            using (var buffer = frame.LockBuffer(AudioBufferAccessMode.Read))
            using (var reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;

                ((IMemoryBufferByteAccess) reference).GetBuffer(out dataInBytes, out capacityInBytes);

                var capacityInFloats = capacityInBytes/4;
                if (capacityInFloats != _frameSize) // Only send frames with the correct size.
                    return;

                var dataInFloats = (float*) dataInBytes;
                var floats = new float[capacityInFloats];
                Marshal.Copy((IntPtr) dataInFloats, floats, 0, (int) capacityInFloats);

                var shorts = ConvertFloatsToShorts(floats);

                ToxAvModel.Instance.SendAudioFrame(_friendNumber, new ToxAvAudioFrame(shorts, _samplingRate, 1));
            }
        }

        private short[] ConvertFloatsToShorts(float[] inSamples)
        {
            var outSamples = new short[inSamples.Length];

            for (var i = 0; i < inSamples.Length; i++)
            {
                float dtmp;
                if (inSamples[i] >= 0)
                {
                    dtmp = inSamples[i] + 0.5f;
                    if (dtmp > short.MaxValue)
                    {
                        dtmp = short.MaxValue;
                    }
                }
                else
                {
                    dtmp = inSamples[i] - 0.5f;
                    if (dtmp < short.MinValue)
                    {
                        dtmp = short.MinValue;
                    }
                }
                outSamples[i] = (short) (dtmp);
            }
            return outSamples;
        }

        #endregion

        #region Constants

        // By default it's 10 ms and we don't intend to modfiy it. See: https://msdn.microsoft.com/en-us/library/windows/apps/xaml/mt203787.aspx#audiograph_class
        private const int KQuantumSize = 10;

        private const string KRingInFileName = "ring-in.wav";
        private const string KRingOutFileName = "ring-out.wav";

        #endregion

        #region Fields

        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
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
                       (_startCallCommand = new RelayCommand(async () =>
                       {
                           await StartAudioGraph();
                           ToxAvModel.Instance.Call(_friendNumber, _bitRate, 0);
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
                           StopAudioGraph();
                           ToxAvModel.Instance.SendControl(_friendNumber, ToxAvCallControl.Cancel);
                           State = CallState.Default;
                           _friendIsReceivingAudio = false;
                       }));
            }
        }

        #endregion
    }
}