using System;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;
using WinTox.Model;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel.Messaging
{
    public class SentMessageViewModelBase : ToxMessageViewModelBase
    {
        private readonly CoreDispatcher _dispatcher;
        private readonly FriendViewModel _target;
        private Timer _resendTimer;
        private int _timerCallbackFired;

        public SentMessageViewModelBase(string text, DateTime timestamp, ToxMessageType messageType, int id,
            FriendViewModel target)
        {
            Text = text;
            Timestamp = timestamp;
            MessageType = messageType;
            Sender = new UserViewModel();
            IsDelivered = false;
            Id = id;
            _target = target;

            _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            ToxModel.Instance.ReadReceiptReceived += ReadReceiptReceivedHandler;
            ToxModel.Instance.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;

            SetupAndStartResendTimer();
        }

        public int Id { get; private set; }

        /// <summary>
        /// We automatically resend the message every 10 seconds for 3 times if we don't get a read receipt
        /// for it in 10 seconds after we sent it for the first time.
        /// </summary>
        private void SetupAndStartResendTimer()
        {
            if (ToxModel.Instance.IsFriendOnline(_target.FriendNumber))
            {
                _timerCallbackFired = 0;

                _resendTimer = new Timer(
                    state =>
                    {
                        if (IsDelivered) // If it's delivered, then there's no need for resend.
                        {
                            _resendTimer.Dispose();
                            return;
                        }

                        ResendMessage();

                        _timerCallbackFired++;
                        if (_timerCallbackFired == 3) // Don't resend it automatically more than 3 times.
                            _resendTimer.Dispose();
                    },
                    null, 10000, 10000);
            }
        }

        private async void ReadReceiptReceivedHandler(object sender, ToxEventArgs.ReadReceiptEventArgs e)
        {
            if (e.FriendNumber != _target.FriendNumber)
                return;

            if (Id == e.Receipt)
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { IsDelivered = true; });
            }
        }

        private void FriendConnectionStatusChangedHandler(object sender, ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (e.FriendNumber != _target.FriendNumber)
                return;

            if (!IsDelivered)
                ResendMessage();
        }

        private void ResendMessage()
        {
            var messageId = ToxModel.Instance.SendMessage(_target.FriendNumber, Text, MessageType);
            Id = messageId; // We have to update the message ID.
        }
    }
}