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

        public CallBlock()
        {
            InitializeComponent();
        }

        private void AudioCallBlockLoaded(object sender, RoutedEventArgs e)
        {
            _audioCallViewModel = ((CallViewModel) DataContext).Audio;
            _audioCallViewModel.MicrophoneIsNotAvailable += MicrophoneIsNotAvailableHandler;
            _audioCallViewModel.StartRinging += StartRingingHandler;
            _audioCallViewModel.StopRinging += StopRingingHandler;
            _audioCallViewModel.PropertyChanged += PropertyChangedHandler;

            var state =
                (string)
                    new CallStateToStringConverter().Convert(_audioCallViewModel.State, typeof (string), null, null);
            VisualStateManager.GoToState(this, state, false);
        }

        private void AudioCallBlockUnloaded(object sender, RoutedEventArgs e)
        {
            _audioCallViewModel.MicrophoneIsNotAvailable -= MicrophoneIsNotAvailableHandler;
            _audioCallViewModel.StartRinging -= StartRingingHandler;
            _audioCallViewModel.StopRinging -= StopRingingHandler;
            _audioCallViewModel.PropertyChanged -= PropertyChangedHandler;
        }

        #region Ring event handlers

        private void StartRingingHandler(object sender, string ringFileName)
        {
            RingPlayer.Source = new Uri("ms-appx:///Assets/" + ringFileName);
        }

        private void StopRingingHandler(object sender, EventArgs e)
        {
            RingPlayer.Stop();
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
                if (_microphoneIsNotAvailableFylout != null)
                {
                    _microphoneIsNotAvailableFylout.Hide();
                }
            }
        }

        #endregion
    }
}