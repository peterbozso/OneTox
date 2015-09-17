using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using OneTox.Model;
using SharpTox.Av;

namespace OneTox.ViewModel.Calls
{
    public class AudioCallViewModel : ViewModelBase
    {
        public enum CallState
        {
            Default,
            DuringCall,
            OutgoingCall,
            IncomingCall
        }

        [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal unsafe interface IMemoryBufferByteAccess
        {
            void GetBuffer(out byte* buffer, out uint capacity);
        }

        #region Constructor

        public AudioCallViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;

            InitializeRates();

            ToxAvModel.Instance.CallStateChanged += CallStateChangedHandler;
            ToxAvModel.Instance.CallRequestReceived += CallRequestReceivedHandler;
            ToxAvModel.Instance.AudioFrameReceived += AudioFrameReceivedHandler;
        }

        private void InitializeRates()
        {
            // TODO: Set these based on app settings!
            _bitRate = 48;
            _samplingRate = _bitRate*1000;
            _frameSize = _samplingRate*KQuantumSize/1000;
        }

        #endregion Constructor

        #region Microphone availability error

        public event EventHandler<string> MicrophoneIsNotAvailable;

        private void RaiseMicrophoneIsNotAvailable(string errorMessage)
        {
            MicrophoneIsNotAvailable?.Invoke(this, errorMessage);
        }

        #endregion Microphone availability error

        #region ToxAv event handlers

        private void CallRequestReceivedHandler(object sender, ToxAvEventArgs.CallRequestEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => { State = CallState.IncomingCall; });
        }

        private void CallStateChangedHandler(object sender, ToxAvEventArgs.CallStateEventArgs e)
        {
            if (e.FriendNumber != _friendNumber)
                return;

            if (e.State.HasFlag(ToxAvFriendCallState.ReceivingAudio) ||
                e.State.HasFlag(ToxAvFriendCallState.SendingAudio))
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => { State = CallState.DuringCall; });
            }

            _friendIsReceivingAudio = e.State.HasFlag(ToxAvFriendCallState.ReceivingAudio);

            if (e.State.HasFlag(ToxAvFriendCallState.Finished) || e.State.HasFlag(ToxAvFriendCallState.Error))
            {
                StopAudioGraph();
                DispatcherHelper.CheckBeginInvokeOnUI(() => { State = CallState.Default; });
            }
        }

        #endregion ToxAv event handlers

        #region Fields

        private readonly int _friendNumber;
        private AudioGraph _audioGraph;
        private int _bitRate;
        private int _frameSize;
        private bool _friendIsReceivingAudio;
        private AudioDeviceInputNode _microphoneInputNode;
        private int _samplingRate;
        private AudioDeviceOutputNode _speakerOutputNode;
        private AudioFrameInputNode _toxInputNode;
        private AudioFrameOutputNode _toxOutputNode;

        #endregion Fields

        #region AudioGraph

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

        private async Task StartAudioGraph()
        {
            if (_audioGraph == null)
            {
                await InitAudioGraph();

                var success = await CreateMicrophoneInputNode();
                if (!success)
                {
                    IsMuted = true;
                    return;
                }

                CreateToxOutputNode();

                CreateToxInputNode();
                await CreateSpeakerOutputNode();
            }
            else
            {
                if (IsMuted)
                {
                    _microphoneInputNode.Stop();
                    _toxOutputNode.Stop();
                }
                else
                {
                    _microphoneInputNode.Start();
                    _toxOutputNode.Start();
                }
            }

            _audioGraph.Start();
        }

        private void StopAudioGraph()
        {
            _audioGraph?.Stop();
        }

        #endregion AudioGraph

        #region Audio sending

        private void AudioGraphQuantumProcessedHandler(AudioGraph sender, object args)
        {
            if (!_friendIsReceivingAudio)
                return;

            var frame = _toxOutputNode.GetFrame();
            ProcessFrameOutput(frame);
        }

        private short[] ConvertFloatsToShorts(float[] inSamples)
        {
            var outSamples = new short[inSamples.Length];

            for (var i = 0; i < inSamples.Length; i++)
            {
                var temp = inSamples[i]*(short.MaxValue + 1);

                if (temp > short.MaxValue)
                {
                    temp = short.MaxValue;
                }
                else if (temp < short.MinValue)
                {
                    temp = short.MinValue;
                }

                outSamples[i] = (short) (temp);
            }

            return outSamples;
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

        #endregion Audio sending

        #region Audio receiving

        private void AudioFrameReceivedHandler(object sender, ToxAvEventArgs.AudioFrameEventArgs e)
        {
            if (e.FriendNumber != _friendNumber)
                return;

            _receiveBuffer.Post(e.Frame.Data);
        }

        private float[] ConvertShortsToFloats(short[] inSamples)
        {
            var outSamples = new float[inSamples.Length];

            for (var i = 0; i < inSamples.Length; i++)
            {
                var temp = inSamples[i]/(float) (short.MaxValue + 1);

                if (temp > 1)
                {
                    temp = 1;
                }
                else if (temp < -1)
                {
                    temp = -1;
                }

                outSamples[i] = temp;
            }

            return outSamples;
        }

        private async Task CreateSpeakerOutputNode()
        {
            var result = await _audioGraph.CreateDeviceOutputNodeAsync();

            if (result.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // TODO: Error handling!
                // Cannot create device output node
                // ShowErrorMessage(result.Status.ToString());
                return;
            }

            _speakerOutputNode = result.DeviceOutputNode;
            _toxInputNode.AddOutgoingConnection(_speakerOutputNode);
        }

        private void CreateToxInputNode()
        {
            // Create the FrameInputNode at the same format as the graph, except explicitly set mono.
            var nodeEncodingProperties = _audioGraph.EncodingProperties;
            nodeEncodingProperties.ChannelCount = 1;
            _toxInputNode = _audioGraph.CreateFrameInputNode(nodeEncodingProperties);

            // Hook up an event handler so we can start generating samples when needed
            // This event is triggered when the node is required to provide data
            _toxInputNode.QuantumStarted += ToxInputNodeQuantumStartedHandler;
        }

        // TODO: Fix frame receiving!
        private unsafe AudioFrame GenerateAudioData(uint samples, short[] shorts)
        {
            // Buffer size is (number of samples) * (size of each sample)
            // We choose to generate single channel (mono) audio. For multi-channel, multiply by number of channels
            var bufferSize = samples*sizeof (float);
            var frame = new AudioFrame(bufferSize);

            using (var buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (var reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;

                // Get the buffer from the AudioFrame
                ((IMemoryBufferByteAccess) reference).GetBuffer(out dataInBytes, out capacityInBytes);

                // Cast to float since the data we are generating is float
                var dataInFloats = (float*) dataInBytes;

                var floats = ConvertShortsToFloats(shorts);
                var capacityInFloats = capacityInBytes/4;

                Marshal.Copy(floats, 0, (IntPtr) dataInFloats, (int) capacityInFloats);
            }

            return frame;
        }

        private async void ToxInputNodeQuantumStartedHandler(AudioFrameInputNode sender,
            FrameInputNodeQuantumStartedEventArgs args)
        {
            if (!await _receiveBuffer.OutputAvailableAsync())
                return;

            short[] shorts;
            var successfulReceive = _receiveBuffer.TryReceive(out shorts);
            if (!successfulReceive)
                return;

            // GenerateAudioData can provide PCM audio data by directly synthesizing it or reading from a file.
            // Need to know how many samples are required. In this case, the node is running at the same rate as the rest of the graph
            // For minimum latency, only provide the required amount of samples. Extra samples will introduce additional latency.
            var numSamplesNeeded = (uint) args.RequiredSamples;
            if (numSamplesNeeded == 0)
                return;

            var audioData = GenerateAudioData(numSamplesNeeded, shorts);
            _toxInputNode.AddFrame(audioData);
        }

        #endregion Audio receiving

        #region Constants

        // By default it's 10 ms and we don't intend to modfiy it. See: https://msdn.microsoft.com/en-us/library/windows/apps/xaml/mt203787.aspx#audiograph_class
        private const int KQuantumSize = 10;

        private const string KRingInFileName = "ring-in.wav";
        private const string KRingOutFileName = "ring-out.wav";

        #endregion Constants

        #region Fields

        private readonly BufferBlock<short[]> _receiveBuffer = new BufferBlock<short[]>();
        private RelayCommand _acceptCallCommand;
        private RelayCommand _changeMuteCommand;
        private RelayCommand _declineCallCommand;
        private bool _isMuted;
        private RelayCommand _startCallCommand;
        private CallState _state;
        private RelayCommand _stopCallCommand;

        #endregion Fields

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

        #endregion Properties

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

        #endregion Ringing events

        #region Commands

        public RelayCommand AcceptCallCommand
        {
            get
            {
                return _acceptCallCommand ?? (_acceptCallCommand = new RelayCommand(async () =>
                {
                    await StartAudioGraph();
                    ToxAvModel.Instance.Answer(_friendNumber, _bitRate, 0);
                    State = CallState.DuringCall;
                }));
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
                        _microphoneInputNode.Stop();
                        _toxOutputNode.Stop();
                    }
                    else
                    {
                        _microphoneInputNode.Start();
                        _toxOutputNode.Start();
                    }
                }));
            }
        }

        public RelayCommand DeclineCallCommand
        {
            get
            {
                return _declineCallCommand ?? (_declineCallCommand = new RelayCommand(() =>
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
                       (_startCallCommand = new RelayCommand(async () =>
                       {
                           // TODO: Give opportunity to the user to cancel his/her ongoing call in order to initiate a new one with this friend.
                           if (!ToxAvModel.Instance.CanCall)
                               return;

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

        #endregion Commands
    }
}