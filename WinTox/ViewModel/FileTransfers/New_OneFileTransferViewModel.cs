using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WinTox.Model;

namespace WinTox.ViewModel.FileTransfers
{
    public class New_OneFileTransferViewModel : ViewModelBase
    {
        private readonly ProgressUpdater _progressUpdater;
        private readonly OneFileTransferModel _transferModel;
        private double _progress;

        public New_OneFileTransferViewModel(OneFileTransferModel fileTransferModel)
        {
            _transferModel = fileTransferModel;
            _transferModel.PropertyChanged += ModelPropertyChangedHandler;
            _progressUpdater = new ProgressUpdater(this);
        }

        public string Name
        {
            get { return _transferModel.Name; }
        }

        public TransferDirection Direction
        {
            get { return _transferModel.Direction; }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (value.Equals(_progress))
                    return;
                _progress = value;
                RaisePropertyChanged();
            }
        }

        public FileTransferState State
        {
            get { return _transferModel.State; }
        }

        private async void ModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            await
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { RaisePropertyChanged(e.PropertyName); });
        }

        public void UpDateProgress()
        {
            Progress = _transferModel.Progress;
        }

        #region Progress updater

        /// <summary>
        ///     This class's purpose is to update the progress bars' progress on FileTransfersBlock.
        /// </summary>
        private class ProgressUpdater
        {
            private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            private readonly New_OneFileTransferViewModel _fileTransferViewModel;
            private readonly DispatcherTimer _progressDispatcherTimer;

            public ProgressUpdater(New_OneFileTransferViewModel fileTransferViewModel)
            {
                _fileTransferViewModel = fileTransferViewModel;
                _fileTransferViewModel.PropertyChanged += StateChangedHandler;

                _progressDispatcherTimer = new DispatcherTimer();
                _progressDispatcherTimer.Tick += ProgressDispatcherTimerTickHandler;
                _progressDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            }

            private async void StateChangedHandler(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != "State")
                    return;

                _fileTransferViewModel.UpDateProgress();

                if (_fileTransferViewModel.State != FileTransferState.Uploading &&
                    _fileTransferViewModel.State != FileTransferState.Downloading)
                {
                    await
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { _progressDispatcherTimer.Stop(); });
                }
                else
                {
                    await
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { _progressDispatcherTimer.Start(); });
                }
            }

            /// <summary>
            ///     On every tick, we update the progress of each OneFileTransferViewModel. We need to do this periodically to not to
            ///     block the UI thread and maintain a fluid display of progress changes.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void ProgressDispatcherTimerTickHandler(object sender, object e)
            {
                _fileTransferViewModel.UpDateProgress();
            }
        }

        #endregion
    }
}