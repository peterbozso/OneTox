using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
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
        }

        private void InitializeRates()
        {
            _bitRate = 48;
            _samplingRate = 48*1000;
        }

        public event EventHandler<string> MicrophoneIsNotAvailable;

        #region Fields

        private readonly int _friendNumber;
        private AudioGraph _audioGraph;
        private AudioDeviceInputNode _deviceInputNode;
        private AudioFrameOutputNode _frameOutputNode;
        private int _samplingRate;
        private int _bitRate;

        #endregion

        #region Audio sending

        private async Task StartAudioGraph()
        {
            await InitAudioGraph();
            await CreateDeviceInputNode();
            CreateFrameOutputNode();
            _audioGraph.Start();
        }

        private async Task InitAudioGraph()
        {
            var encodingProperties = AudioEncodingProperties.CreatePcm((uint) _samplingRate, 1, 16);

            var settings = new AudioGraphSettings(AudioRenderCategory.Communications)
            {
                EncodingProperties = encodingProperties
            };

            var result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                // TODO: Error handling!
                // ShowErrorMessage("AudioGraph creation error: " + result.Status.ToString());
                return;
            }

            _audioGraph = result.Graph;
        }

        private async Task CreateDeviceInputNode()
        {
            // Create a device output node
            var result = await _audioGraph.CreateDeviceInputNodeAsync(MediaCategory.Communications);

            if (result.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // TODO: Error handling!
                // Cannot create device output node
                // ShowErrorMessage(result.Status.ToString());
                return;
            }

            _deviceInputNode = result.DeviceInputNode;
        }

        private void CreateFrameOutputNode()
        {
            _frameOutputNode = _audioGraph.CreateFrameOutputNode();
            _audioGraph.QuantumProcessed += AudioGraphQuantumProcessed;
            _deviceInputNode.AddOutgoingConnection(_frameOutputNode);
        }

        private void AudioGraphQuantumProcessed(AudioGraph sender, object args)
        {
            var frame = _frameOutputNode.GetFrame();
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

                if (capacityInBytes == 0) // Don't send empty frames.
                    return;

                var bytes = new byte[capacityInBytes];
                Marshal.Copy((IntPtr) dataInBytes, bytes, 0, (int) capacityInBytes);

                var shorts = new short[capacityInBytes/2];
                Buffer.BlockCopy(bytes, 0, shorts, 0, (int) capacityInBytes);

                ToxAvModel.Instance.SendAudioFrame(_friendNumber, new ToxAvAudioFrame(shorts, _samplingRate, 1));
            }
        }

        #endregion

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
                       (_startCallCommand = new RelayCommand(async () =>
                       {
                           ToxAvModel.Instance.Call(_friendNumber, _bitRate, 0);
                           await StartAudioGraph();
                       }));
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