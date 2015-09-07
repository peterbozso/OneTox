using System;
using System.Threading;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using OneTox.Config;
using OneTox.Model.Tox;
using OneTox.ViewModel.Friends;
using SharpTox.Core;

namespace OneTox.ViewModel.Messaging
{
    public class SentMessageViewModel : ToxMessageViewModelBase
    {
        private readonly FriendViewModel _target;
        private readonly IToxModel _toxModel;
        private RelayCommand _resendMessageCommand;
        private Timer _resendTimer;
        private int _timerCallbackFired;

        public SentMessageViewModel(IDataService dataService, string text, DateTime timestamp,
            ToxMessageType messageType, int id,
            FriendViewModel target)
        {
            _toxModel = dataService.ToxModel;

            Text = text;
            Timestamp = timestamp;
            MessageType = messageType;
            Sender = new UserViewModel(dataService);
            State = MessageDeliveryState.Pending;
            Id = id;
            _target = target;

            _toxModel.ReadReceiptReceived += ReadReceiptReceivedHandler;
            _toxModel.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;

            SetupAndStartResendTimer();
        }

        public int Id { get; private set; }

        public RelayCommand ResendMessageCommand
        {
            get
            {
                return _resendMessageCommand ?? (_resendMessageCommand = new RelayCommand(() =>
                {
                    State = MessageDeliveryState.Pending;
                    SetupAndStartResendTimer();
                }));
            }
        }

        private void FriendConnectionStatusChangedHandler(object sender, ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (e.FriendNumber != _target.FriendNumber)
                return;

            if (State == MessageDeliveryState.Pending)
                ResendMessage();
        }

        private void ReadReceiptReceivedHandler(object sender, ToxEventArgs.ReadReceiptEventArgs e)
        {
            if (e.FriendNumber != _target.FriendNumber)
                return;

            if (Id == e.Receipt)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => { State = MessageDeliveryState.Delivered; });
            }
        }

        private void ResendMessage()
        {
            var messageId = _toxModel.SendMessage(_target.FriendNumber, Text, MessageType);
            Id = messageId; // We have to update the message ID.
        }

        /// <summary>
        ///     We automatically resend the message every 10 seconds for 3 times if we don't get a read receipt
        ///     for it in 10 seconds after we sent it for the first time.
        /// </summary>
        private void SetupAndStartResendTimer()
        {
            if (_toxModel.IsFriendOnline(_target.FriendNumber))
            {
                _timerCallbackFired = 0;

                _resendTimer = new Timer(
                    state =>
                    {
                        if (State == MessageDeliveryState.Delivered)
                            // If it's delivered, then there's no need for resend.
                        {
                            _resendTimer.Dispose();
                            return;
                        }

                        ResendMessage();

                        _timerCallbackFired++;
                        if (_timerCallbackFired == 3) // Don't resend it automatically more than 3 times.
                        {
                            _resendTimer.Dispose();
                            State = MessageDeliveryState.Failed;
                        }
                    },
                    null, 10000, 10000);
            }
        }
    }
}