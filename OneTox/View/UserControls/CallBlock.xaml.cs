using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OneTox.View.Converters;
using OneTox.ViewModel.Calls;

namespace OneTox.View.UserControls
{
    public sealed partial class CallBlock : UserControl
    {
        private AudioCallViewModel _audioCallViewModel;
        private Flyout _microphoneIsNotAvailableFylout;
        private RingManager _ringManager;

        public CallBlock()
        {
            InitializeComponent();
        }

        private void AudioCallBlockLoaded(object sender, RoutedEventArgs e)
        {
            _audioCallViewModel = ((CallViewModel) DataContext).Audio;
            _audioCallViewModel.MicrophoneIsNotAvailable += MicrophoneIsNotAvailableHandler;
            _audioCallViewModel.PropertyChanged += PropertyChangedHandler;

            _ringManager = new RingManager(RingPlayer);
            _ringManager.RegisterEventHandlers(_audioCallViewModel);

            var state =
                (string)
                    new CallStateToStringConverter().Convert(_audioCallViewModel.State, typeof (string), null, null);
            VisualStateManager.GoToState(this, state, false);
        }

        private void AudioCallBlockUnloaded(object sender, RoutedEventArgs e)
        {
            _audioCallViewModel.MicrophoneIsNotAvailable -= MicrophoneIsNotAvailableHandler;
            _audioCallViewModel.PropertyChanged -= PropertyChangedHandler;

            _ringManager.DeregisterEventHandlers(_audioCallViewModel);
        }

        #region RingManager

        private class RingManager
        {
            private readonly MediaElement _ringPlayer;
            private bool _isRinging;

            public RingManager(MediaElement ringPlayer)
            {
                _ringPlayer = ringPlayer;
                ringPlayer.MediaOpened += MediaOpenedHandler;
            }

            public void RegisterEventHandlers(AudioCallViewModel audioCallViewModel)
            {
                audioCallViewModel.StartRinging += StartRingingHandler;
                audioCallViewModel.StopRinging += StopRingingHandler;
            }

            public void DeregisterEventHandlers(AudioCallViewModel audioCallViewModel)
            {
                audioCallViewModel.StartRinging -= StartRingingHandler;
                audioCallViewModel.StopRinging -= StopRingingHandler;
            }

            private void StartRingingHandler(object sender, string ringFileName)
            {
                _isRinging = true;
                _ringPlayer.Source = new Uri("ms-appx:///Assets/" + ringFileName);
            }

            private void StopRingingHandler(object sender, EventArgs e)
            {
                _isRinging = false;
                _ringPlayer.Stop();
            }

            /// <summary>
            ///     Sometimes (for example in case of an echobot) the friend answers our call so soon that the ringing sound gets stuck
            ///     if RingPlayer is on autoplay mode.
            ///     It's because it takes time to load the Source sound file and during that time our call gets answered. So the
            ///     desired behavior for the app would be to doesn't even start(!) playing the ringing sound.
            ///     To achieve that we do a double check: we don't start playing the ringing sound right after it's loaded. We first
            ///     check if it's still desired to play it (_isRinging is true), and act based on that.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void MediaOpenedHandler(object sender, RoutedEventArgs e)
            {
                if (_isRinging)
                    _ringPlayer.Play();
            }
        }

        #endregion

        #region Microphone availability error handling

        private void MicrophoneIsNotAvailableHandler(object sender, string errorMessage)
        {
            if (_microphoneIsNotAvailableFylout == null)
            {
                var contentGrid = GetMicrophoneIsNotAvailableFlyoutContent(errorMessage);
                _microphoneIsNotAvailableFylout = new Flyout {Content = contentGrid};
            }
            _microphoneIsNotAvailableFylout.ShowAt(MuteButton);
        }

        private Grid GetMicrophoneIsNotAvailableFlyoutContent(string errorMessage)
        {
            var contentGrid = new Grid {Width = 300};

            contentGrid.Children.Add(new TextBlock
            {
                Text = errorMessage,
                FontSize = 15,
                TextWrapping = TextWrapping.WrapWholeWords
            });

            return contentGrid;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            // Hide the flyout when the call ends.
            if (e.PropertyName == "State" && _audioCallViewModel.State == AudioCallViewModel.CallState.Default)
            {
                _microphoneIsNotAvailableFylout?.Hide();
            }
        }

        #endregion
    }
}