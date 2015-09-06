using System;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using OneTox.Helpers;
using OneTox.Model.FileTransfers;

namespace OneTox.ViewModel.FileTransfers
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
        private readonly OneFileTransferModel _oneFileTransferModel;
        private readonly ProgressUpdater _progressUpdater;
        private RelayCommand _acceptTransferCommand;
        private RelayCommand _cancelTransferCommand;
        private RelayCommand _pauseTransferCommand;
        private double _progress;
        private RelayCommand _resumeTransferCommand;

        #endregion Fields

        #region Properties

        public string Name => _oneFileTransferModel.Name;

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

        public FileTransferState State => _oneFileTransferModel.State;

        private void ModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => { RaisePropertyChanged(e.PropertyName); });
        }

        #endregion Properties

        #region Commands

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

        #endregion Commands

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

            private void StateChangedHandler(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != "State")
                    return;

                _fileTransferViewModel.UpDateProgress();

                if (_fileTransferViewModel.State != FileTransferState.Uploading &&
                    _fileTransferViewModel.State != FileTransferState.Downloading)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => { _progressDispatcherTimer.Stop(); });
                }
                else
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => { _progressDispatcherTimer.Start(); });
                }
            }
        }

        #endregion Progress updater
    }
}