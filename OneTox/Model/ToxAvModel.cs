using System;
using OneTox.ViewModel;
using SharpTox.Av;

namespace OneTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class ToxAvModel
    {
        private static ToxAvModel _instance;
        private ToxAv _toxAv;
        public static ToxAvModel Instance => _instance ?? (_instance = new ToxAvModel());

        #region Methods

        public void SetCurrent(ExtendedTox tox)
        {
            _toxAv?.Dispose();

            _toxAv = new ToxAv(tox);

            RegisterHandlers();
        }

        public void Start()
        {
            _toxAv.Start();
        }

        private void RegisterHandlers()
        {
            _toxAv.OnCallRequestReceived += CallRequestReceivedHandler;
            _toxAv.OnCallStateChanged += CallStateChangedHandler;
            _toxAv.OnAudioBitrateChanged += AudioBitrateChangedHandler;
            _toxAv.OnVideoBitrateChanged += VideoBitrateChangedHandler;
            _toxAv.OnAudioFrameReceived += AudioFrameReceivedHandler;
            _toxAv.OnVideoFrameReceived += VideoFrameReceivedHandler;
        }

        public bool Call(int friendNumber, int audioBitrate, int videoBitrate)
        {
            ToxAvErrorCall error;
            var retVal = _toxAv.Call(friendNumber, audioBitrate, videoBitrate, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public bool Answer(int friendNumber, int audioBitrate, int videoBitrate)
        {
            ToxAvErrorAnswer error;
            var retVal = _toxAv.Answer(friendNumber, audioBitrate, videoBitrate, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public bool SendControl(int friendNumber, ToxAvCallControl control)
        {
            ToxAvErrorCallControl error;
            var retVal = _toxAv.SendControl(friendNumber, control, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public bool SetAudioBitrate(int friendNumber, int bitrate, bool force)
        {
            ToxAvErrorSetBitrate error;
            var retVal = _toxAv.SetAudioBitrate(friendNumber, bitrate, force, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public bool SetVideoBitrate(int friendNumber, int bitrate, bool force)
        {
            ToxAvErrorSetBitrate error;
            var retVal = _toxAv.SetVideoBitrate(friendNumber, bitrate, force, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public bool SendVideoFrame(int friendNumber, ToxAvVideoFrame frame)
        {
            ToxAvErrorSendFrame error;
            var retVal = _toxAv.SendVideoFrame(friendNumber, frame, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public bool SendAudioFrame(int friendNumber, ToxAvAudioFrame frame)
        {
            ToxAvErrorSendFrame error;
            var retVal = _toxAv.SendAudioFrame(friendNumber, frame, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        #endregion

        #region Events

        public event EventHandler<ToxAvEventArgs.CallRequestEventArgs> CallRequestReceived;

        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> CallStateChanged;

        public event EventHandler<ToxAvEventArgs.BitrateStatusEventArgs> AudioBitrateChanged;

        public event EventHandler<ToxAvEventArgs.BitrateStatusEventArgs> VideoBitrateChanged;

        public event EventHandler<ToxAvEventArgs.AudioFrameEventArgs> AudioFrameReceived;

        public event EventHandler<ToxAvEventArgs.VideoFrameEventArgs> VideoFrameReceived;

        #endregion

        #region Event handlers

        private void CallRequestReceivedHandler(object sender, ToxAvEventArgs.CallRequestEventArgs e)
        {
            CallRequestReceived?.Invoke(this, e);
        }

        private void CallStateChangedHandler(object sender, ToxAvEventArgs.CallStateEventArgs e)
        {
            CallStateChanged?.Invoke(this, e);
        }

        private void AudioBitrateChangedHandler(object sender, ToxAvEventArgs.BitrateStatusEventArgs e)
        {
            AudioBitrateChanged?.Invoke(this, e);
        }

        private void VideoBitrateChangedHandler(object sender, ToxAvEventArgs.BitrateStatusEventArgs e)
        {
            VideoBitrateChanged?.Invoke(this, e);
        }

        private void AudioFrameReceivedHandler(object sender, ToxAvEventArgs.AudioFrameEventArgs e)
        {
            AudioFrameReceived?.Invoke(this, e);
        }

        private void VideoFrameReceivedHandler(object sender, ToxAvEventArgs.VideoFrameEventArgs e)
        {
            VideoFrameReceived?.Invoke(this, e);
        }

        #endregion
    }
}