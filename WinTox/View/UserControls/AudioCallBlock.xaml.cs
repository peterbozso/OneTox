using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTox.Converters;
using WinTox.ViewModel;

namespace WinTox.View.UserControls
{
    public sealed partial class AudioCallBlock : UserControl
    {
        private CallViewModel _callViewModel;
        private Flyout _microphoneIsNotAvailableFylout;

        public AudioCallBlock()
        {
            InitializeComponent();
        }

        private void AudioCallBlockLoaded(object sender, RoutedEventArgs e)
        {
            _callViewModel = (CallViewModel) DataContext;
            _callViewModel.MicrophoneIsNotAvailable += MicrophoneIsNotAvailableHandler;
            _callViewModel.StartRinging += StartRingingHandler;
            _callViewModel.StopRinging += StopRingingHandler;
            _callViewModel.PropertyChanged += PropertyChangedHandler;

            var state =
                (string) new CallStateToStringConverter().Convert(_callViewModel.State, typeof (string), null, null);
            VisualStateManager.GoToState(this, state, false);
        }

        private void AudioCallBlockUnloaded(object sender, RoutedEventArgs e)
        {
            _callViewModel = (CallViewModel) DataContext;
            _callViewModel.MicrophoneIsNotAvailable -= MicrophoneIsNotAvailableHandler;
            _callViewModel.StartRinging -= StartRingingHandler;
            _callViewModel.StopRinging -= StopRingingHandler;
            _callViewModel.PropertyChanged -= PropertyChangedHandler;
        }

        private void StartRingingHandler(object sender, string ringFileName)
        {
            RingPlayer.Source = new Uri("ms-appx:///Assets/" + ringFileName);
        }

        private void StopRingingHandler(object sender, EventArgs e)
        {
            RingPlayer.Stop();
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State" && _callViewModel.State == CallViewModel.CallState.Default)
            {
                if (_microphoneIsNotAvailableFylout != null)
                {
                    _microphoneIsNotAvailableFylout.Hide();
                }
            }
        }

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
    }
}