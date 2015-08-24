using OneTox.ViewModel;
using SharpTox.Av;
using System;

namespace OneTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class ToxAvModel
    {
        private static ToxAvModel _instance;

        /// <summary>
        ///     When equals to -1, then there's no ongoing call. Otherwise it contains the friend number of the friend who is
        ///     currently in call with the user.
        /// </summary>
        private int _friendInCall = -1;

        private ToxAv _toxAv;
        public bool CanCall => _friendInCall == -1;
        public static ToxAvModel Instance => _instance ?? (_instance = new ToxAvModel());

        #region Methods

        public bool Answer(int friendNumber, int audioBitrate, int videoBitrate)
        {
            ToxAvErrorAnswer error;
            var retVal = _toxAv.Answer(friendNumber, audioBitrate, videoBitrate, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public bool Call(int friendNumber, int audioBitrate, int videoBitrate)
        {
            if (_friendInCall != -1)
                return false;

            _friendInCall = friendNumber;

            ToxAvErrorCall error;
            var retVal = _toxAv.Call(friendNumber, audioBitrate, videoBitrate, out error);
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

        public bool SendControl(int friendNumber, ToxAvCallControl control)
        {
            if (friendNumber == _friendInCall && control == ToxAvCallControl.Cancel)
                _friendInCall = -1;

            ToxAvErrorCallControl error;
            var retVal = _toxAv.SendControl(friendNumber, control, out error);
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

        public bool SetAudioBitrate(int friendNumber, int bitrate, bool force)
        {
            ToxAvErrorSetBitrate error;
            var retVal = _toxAv.SetAudioBitrate(friendNumber, bitrate, force, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
        }

        public void SetCurrent(ExtendedTox tox)
        {
            _toxAv?.Dispose();

            _toxAv = new ToxAv(tox);

            RegisterHandlers();
        }

        public bool SetVideoBitrate(int friendNumber, int bitrate, bool force)
        {
            ToxAvErrorSetBitrate error;
            var retVal = _toxAv.SetVideoBitrate(friendNumber, bitrate, force, out error);
            ToxErrorViewModel.Instance.RelayError(error);
            return retVal;
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

        #endregion Methods

        #region Events

        public event EventHandler<ToxAvEventArgs.BitrateStatusEventArgs> AudioBitrateChanged;

        public event EventHandler<ToxAvEventArgs.AudioFrameEventArgs> AudioFrameReceived;

        public event EventHandler<ToxAvEventArgs.CallRequestEventArgs> CallRequestReceived;

        public event EventHandler<ToxAvEventArgs.CallStateEventArgs> CallStateChanged;

        public event EventHandler<ToxAvEventArgs.BitrateStatusEventArgs> VideoBitrateChanged;

        public event EventHandler<ToxAvEventArgs.VideoFrameEventArgs> VideoFrameReceived;

        #endregion Events

        #region Event handlers

        private void AudioBitrateChangedHandler(object sender, ToxAvEventArgs.BitrateStatusEventArgs e)
        {
            AudioBitrateChanged?.Invoke(this, e);
        }

        private void AudioFrameReceivedHandler(object sender, ToxAvEventArgs.AudioFrameEventArgs e)
        {
            AudioFrameReceived?.Invoke(this, e);
        }

        private void CallRequestReceivedHandler(object sender, ToxAvEventArgs.CallRequestEventArgs e)
        {
            // Automatically decline call request if we have an ongoing call.
            // TODO: Instead of this, tell the user about the situation and let him/her decide if he/she want to hang up the current call and answer the new one!
            if (_friendInCall != -1)
            {
                _toxAv.SendControl(e.FriendNumber, ToxAvCallControl.Cancel);
                return;
            }

            _friendInCall = e.FriendNumber;

            CallRequestReceived?.Invoke(this, e);
        }

        private void CallStateChangedHandler(object sender, ToxAvEventArgs.CallStateEventArgs e)
        {
            if ((e.FriendNumber == _friendInCall) &&
                (e.State.HasFlag(ToxAvFriendCallState.Finished) || e.State.HasFlag(ToxAvFriendCallState.Error)))
            {
                _friendInCall = -1;
            }

            CallStateChanged?.Invoke(this, e);
        }

        private void VideoBitrateChangedHandler(object sender, ToxAvEventArgs.BitrateStatusEventArgs e)
        {
            VideoBitrateChanged?.Invoke(this, e);
        }

        private void VideoFrameReceivedHandler(object sender, ToxAvEventArgs.VideoFrameEventArgs e)
        {
            VideoFrameReceived?.Invoke(this, e);
        }

        #endregion Event handlers
    }
}
