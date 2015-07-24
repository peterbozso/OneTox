using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTox.Converters;
using WinTox.ViewModel;

namespace WinTox.View.UserControls
{
    public sealed partial class AudioCallBlock : UserControl
    {
        public AudioCallBlock()
        {
            InitializeComponent();
        }

        private void AudioCallBlockLoaded(object sender, RoutedEventArgs e)
        {
            var callViewModel = (CallViewModel) DataContext;
            callViewModel.StartCallByUserFailed += StartCallByUserFailedHandler;
            callViewModel.StartRinging += StartRingingHandler;
            callViewModel.StopRinging += StopRingingHandler;

            var state =
                (string) new CallStateToStringConverter().Convert(callViewModel.State, typeof (string), null, null);
            VisualStateManager.GoToState(this, state, false);
        }

        private void AudioCallBlockUnloaded(object sender, RoutedEventArgs e)
        {
            var callViewModel = (CallViewModel) DataContext;
            callViewModel.StartCallByUserFailed -= StartCallByUserFailedHandler;
            callViewModel.StartRinging -= StartRingingHandler;
            callViewModel.StopRinging -= StopRingingHandler;
        }

        private void StartRingingHandler(object sender, string ringFileName)
        {
            RingPlayer.Source = new Uri("ms-appx:///Assets/" + ringFileName);
        }

        private void StopRingingHandler(object sender, EventArgs e)
        {
            RingPlayer.Stop();
        }

        private void StartCallByUserFailedHandler(object sender, string errorMessage)
        {
            var contentGrid = GetCallByUserFailedFlyoutContent(errorMessage);
            var flyout = new Flyout {Content = contentGrid};
            flyout.ShowAt(CallButton);
        }

        private Grid GetCallByUserFailedFlyoutContent(string errorMessage)
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