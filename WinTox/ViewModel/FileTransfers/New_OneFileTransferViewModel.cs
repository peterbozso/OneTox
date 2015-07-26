using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WinTox.Common;
using WinTox.Model;

namespace WinTox.ViewModel.FileTransfers
{
    public class New_OneFileTransferViewModel : ViewModelBase
    {
        private readonly New_FileTransfersViewModel _fileTransfersViewModel;
        private readonly ProgressUpdater _progressUpdater;
        private readonly OneFileTransferModel _transferModel;
        private RelayCommand _acceptTransferCommand;
        private RelayCommand _cancelTransferCommand;
        private double _progress;

        public New_OneFileTransferViewModel(New_FileTransfersViewModel fileTransfersViewModel,
            OneFileTransferModel fileTransferModel)
        {
            _fileTransfersViewModel = fileTransfersViewModel;
            _transferModel = fileTransferModel;
            _transferModel.PropertyChanged += ModelPropertyChangedHandler;
            _progressUpdater = new ProgressUpdater(this);
        }

        public string Name
        {
            get { return _transferModel.Name; }
        }

        /// <summary>
        ///     See ProgressUpdater.
        /// </summary>
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

        #region Commands

        public RelayCommand CancelTransferCommand
        {
            get
            {
                return _cancelTransferCommand ?? (_cancelTransferCommand = new RelayCommand(() =>
                {
                    var successFulCancel = _transferModel.CancelTransfer();

                    if (successFulCancel)
                    {
                        _fileTransfersViewModel.Transfers.Remove(this);
                    }
                }));
            }
        }

        public RelayCommand AcceptTransferCommand
        {
            get
            {
                return _acceptTransferCommand ?? (_acceptTransferCommand = new RelayCommand(
                    async () =>
                    {
                        var folderPicker = new FolderPicker();
                        folderPicker.FileTypeFilter.Add("*");
                        var saveFolder = await folderPicker.PickSingleFolderAsync();
                        if (saveFolder == null)
                            return;

                        var saveFile =
                            await saveFolder.CreateFileAsync(Name, CreationCollisionOption.GenerateUniqueName);
                        await _transferModel.AcceptTransfer(saveFile);
                    }));
            }
        }

        #endregion

        #region Progress updater

        public void UpDateProgress()
        {
            Progress = _transferModel.Progress;
        }

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