using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WinTox.Common;
using WinTox.Helpers;
using WinTox.Model.FileTransfers;

namespace WinTox.ViewModel.FileTransfers
{
    public class OneFileTransferViewModel : ObservableObject
    {
        public OneFileTransferViewModel(FileTransfersViewModel fileTransfersViewModel,
            OneFileTransferModel oneFileTransferModel)
        {
            _fileTransfersViewModel = fileTransfersViewModel;
            _oneFileTransferModel = oneFileTransferModel;
            _oneFileTransferModel.PropertyChanged += ModelPropertyChangedHandler;
            _progressUpdater = new ProgressUpdater(this);
        }

        #region Fields

        private readonly FileTransfersViewModel _fileTransfersViewModel;
        private readonly ProgressUpdater _progressUpdater;
        private readonly OneFileTransferModel _oneFileTransferModel;
        private RelayCommand _acceptTransferCommand;
        private RelayCommand _cancelTransferCommand;
        private RelayCommand _pauseTransferCommand;
        private double _progress;
        private RelayCommand _resumeTransferCommand;

        #endregion

        #region Properties

        public string Name
        {
            get { return _oneFileTransferModel.Name; }
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
            get { return _oneFileTransferModel.State; }
        }

        private async void ModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            await
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { RaisePropertyChanged(e.PropertyName); });
        }

        #endregion

        #region Commands

        public RelayCommand CancelTransferCommand
        {
            get
            {
                return _cancelTransferCommand ?? (_cancelTransferCommand = new RelayCommand(() =>
                {
                    _oneFileTransferModel.CancelTransfer();
                    _fileTransfersViewModel.Transfers.Remove(this);
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
                        await _oneFileTransferModel.AcceptTransfer(saveFile);
                    }));
            }
        }

        public RelayCommand PauseTransferCommand
        {
            get
            {
                return _pauseTransferCommand ??
                       (_pauseTransferCommand = new RelayCommand(() => { _oneFileTransferModel.PauseTransfer(); }));
            }
        }

        public RelayCommand ResumeTransferCommand
        {
            get
            {
                return _resumeTransferCommand ??
                       (_resumeTransferCommand = new RelayCommand(() => { _oneFileTransferModel.ResumeTransfer(); }));
            }
        }

        #endregion

        #region Progress updater

        public void UpDateProgress()
        {
            Progress = _oneFileTransferModel.Progress;
        }

        /// <summary>
        ///     This class's purpose is to update the progress bars' progress on FileTransfersBlock.
        /// </summary>
        private class ProgressUpdater
        {
            private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            private readonly OneFileTransferViewModel _fileTransferViewModel;
            private readonly DispatcherTimer _progressDispatcherTimer;

            public ProgressUpdater(OneFileTransferViewModel fileTransferViewModel)
            {
                _fileTransferViewModel = fileTransferViewModel;
                _fileTransferViewModel.PropertyChanged += StateChangedHandler;

                _progressDispatcherTimer = new DispatcherTimer();
                _progressDispatcherTimer.Tick += ProgressDispatcherTimerTickHandler;
                _progressDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

                if (_fileTransferViewModel.State == FileTransferState.Uploading ||
                    _fileTransferViewModel.State == FileTransferState.Downloading)
                {
                    _progressDispatcherTimer.Start();
                }
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