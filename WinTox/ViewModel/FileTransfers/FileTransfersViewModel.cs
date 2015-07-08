using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel.FileTransfers
{
    public class FileTransfersViewModel : ViewModelBase
    {
        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        private readonly ProgressUpdater _progressUpdater;

        public FileTransfersViewModel(int friendNumber)
        {
            FriendNumber = friendNumber;
            Transfers = new ObservableCollection<OneFileTransferViewModel>();
            FileTransferManager.Instance.FileControlReceived += FileControlReceivedHandler;
            FileTransferManager.Instance.TransferFinished += TransferFinishedHandler;
            FileTransferManager.Instance.FileDownloadAdded += FileDownloadAddedHandler;
            FileTransferManager.Instance.FileUploadAdded += FileUploadAddedHandler;
            _progressUpdater = new ProgressUpdater(this);
            VisualStates = new FileTransfersVisualStates();
        }

        public int FriendNumber { get; private set; }
        public ObservableCollection<OneFileTransferViewModel> Transfers { get; private set; }
        public FileTransfersVisualStates VisualStates { get; private set; }

        #region Progress updater

        /// <summary>
        ///     This class's purpose is to update the progress bars' progress on FileTransfersBlock.
        /// </summary>
        private class ProgressUpdater
        {
            private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            private readonly FileTransfersViewModel _fileTransfers;
            private readonly DispatcherTimer _progressDispatcherTimer;

            public ProgressUpdater(FileTransfersViewModel fileTransfers)
            {
                _fileTransfers = fileTransfers;

                _progressDispatcherTimer = new DispatcherTimer();
                _progressDispatcherTimer.Tick += ProgressDispatcherTimerTickHandler;
                _progressDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            }

            /// <summary>
            ///     On every tick, we get the progress of all transfers from FileTransferManager, then update each
            ///     OneFileTransferViewModel accordingly. We need to do this periodically to not to block the UI thread and maintain a
            ///     fluid display of progress changes.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void ProgressDispatcherTimerTickHandler(object sender, object e)
            {
                var progresses = FileTransferManager.Instance.GetTransferProgressesOfFriend(_fileTransfers.FriendNumber);
                foreach (var progress in progresses)
                {
                    var transfer = _fileTransfers.FindNotPlaceHolderTransferViewModel(progress.Key);
                    if (transfer == null)
                        continue;

                    transfer.Progress = progress.Value;
                }
            }

            public async Task StartUpdateIfNeeded()
            {
                if (GetActiveTransfersCount() > 0)
                {
                    await
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { _progressDispatcherTimer.Start(); });
                }
            }

            public async Task StopUpdateIfNeeded()
            {
                if (GetActiveTransfersCount() == 0)
                {
                    await
                        _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { _progressDispatcherTimer.Stop(); });
                }
            }

            private int GetActiveTransfersCount()
            {
                return _fileTransfers.Transfers.Count(transfer => transfer.IsNotPlaceholder &&
                                                                  transfer.State != FileTransferState.PausedByFriend &&
                                                                  transfer.State != FileTransferState.PausedByUser);
            }
        }

        #endregion

        #region Visual states

        /// <summary>
        ///     This class's purpose is to supply (trough data binding) the current visual state of FileTransfersBlock and height
        ///     of OpenContentGrid.
        /// </summary>
        public class FileTransfersVisualStates : ViewModelBase
        {
            /// <summary>
            ///     Open: we have one or more file transfers for the current friend an we show "all" (4 max at once) of them.
            ///     Collapsed: we have one or more file transfers for the current friend and we show a placeholder text instead of
            ///     them.
            ///     Invisible: we have 0 file transfers, so we make FileTransfersBlock invisible.
            ///     The user can switch between Open and Collapsed states manually via the UI. Switching to/from Invisible to/from
            ///     either
            ///     states happens programmatically.
            /// </summary>
            public enum TransfersBlockState
            {
                Open,
                Collapsed,
                Invisible
            }

            private const int KHideArrowTextBlockHeight = 10;
            private const int KFileTransferRibbonHeight = 60;
            private TransfersBlockState _blockState;
            private double _openContentGridHeight;

            public FileTransfersVisualStates()
            {
                BlockState = TransfersBlockState.Invisible;
            }

            public TransfersBlockState BlockState
            {
                get { return _blockState; }
                set
                {
                    _blockState = value;
                    RaisePropertyChanged();
                }
            }

            /// <summary>
            ///     The current height of the OpenContentGrid on FileTransfersBlock. We need this workaround to be able to animate the
            ///     height of the Grid during visual state transitions. It's because to do that, we need concrete heights, what we
            ///     provide with data binding to this property and the 0 constant.
            /// </summary>
            public double OpenContentGridHeight
            {
                get { return _openContentGridHeight; }
                private set
                {
                    _openContentGridHeight = value;
                    RaisePropertyChanged();
                }
            }

            /// <summary>
            ///     Called from FileTransfersViewModel whenever we add or remove a OneFileTransferViewModel (and a FileTransferRibbon
            ///     to/from FileTransfersBlock through data binding) to update OpenContentGridHeight according to the current number of
            ///     file transfers.
            /// </summary>
            /// <param name="transfersCount">The current number of file transfers.</param>
            public void UpdateOpenContentGridHeight(int transfersCount)
            {
                // We don't show more than 4 items in the list at once, but use a scroll bar in that case.
                var itemsToDisplay = transfersCount > 4 ? 4 : transfersCount;
                OpenContentGridHeight = itemsToDisplay*KFileTransferRibbonHeight + KHideArrowTextBlockHeight;
            }
        }

        #endregion

        #region Helper methods

        private OneFileTransferViewModel FindNotPlaceHolderTransferViewModel(int fileNumber)
        {
            return Transfers.FirstOrDefault(transfer => transfer.FileNumber == fileNumber && transfer.IsNotPlaceholder);
            // There can be multiple transfers with the same file number, but there's always only one that's not a placeholder.
        }

        private void AddTransfer(int fileNumber, string fileName, TransferDirection direction)
        {
            Transfers.Add(new OneFileTransferViewModel(this, fileNumber, fileName, direction));

            if (VisualStates.BlockState == FileTransfersVisualStates.TransfersBlockState.Invisible)
                VisualStates.BlockState = FileTransfersVisualStates.TransfersBlockState.Open;

            VisualStates.UpdateOpenContentGridHeight(Transfers.Count);
        }

        private void RemoveTransfer(OneFileTransferViewModel transferViewModel)
        {
            Transfers.Remove(transferViewModel);

            if (Transfers.Count == 0)
                VisualStates.BlockState = FileTransfersVisualStates.TransfersBlockState.Invisible;

            VisualStates.UpdateOpenContentGridHeight(Transfers.Count);
        }

        #endregion

        #region Changes coming from the View, being relayed to the Model

        public async Task SendFile(StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            int fileNumber;

            var successfulSend = FileTransferManager.Instance.SendFile(FriendNumber, stream, file.Name, out fileNumber);

            if (successfulSend)
            {
                AddTransfer(fileNumber, file.Name, TransferDirection.Up);
                FileTransferResumer.Instance.RecordTransfer(file, FriendNumber, fileNumber, TransferDirection.Up);
            }
            else
            {
                stream.Dispose();
            }
        }

        public async Task AcceptTransferByUser(int fileNumber, StorageFile saveFile)
        {
            var saveStream = (await saveFile.OpenAsync(FileAccessMode.ReadWrite)).AsStream();
            FileTransferManager.Instance.ReceiveFile(FriendNumber, fileNumber, saveStream);
            FileTransferResumer.Instance.RecordTransfer(saveFile, FriendNumber, fileNumber, TransferDirection.Down);
            await _progressUpdater.StartUpdateIfNeeded();
        }

        public async Task CancelTransferByUser(OneFileTransferViewModel transferViewModel)
        {
            FileTransferManager.Instance.CancelTransfer(FriendNumber, transferViewModel.FileNumber);
            RemoveTransfer(transferViewModel);
            await _progressUpdater.StopUpdateIfNeeded();
        }

        public async Task PauseTransferByUser(int fileNumber)
        {
            FileTransferManager.Instance.PauseTransfer(FriendNumber, fileNumber);
            await _progressUpdater.StopUpdateIfNeeded();
        }

        public async Task ResumeTransferByUser(int fileNumber)
        {
            FileTransferManager.Instance.ResumeTransfer(FriendNumber, fileNumber);
            await _progressUpdater.StartUpdateIfNeeded();
        }

        #endregion

        #region Changes coming from the Model, being relayed to the View

        private async void FileControlReceivedHandler(object sender, ToxEventArgs.FileControlEventArgs e)
        {
            if (FriendNumber != e.FriendNumber)
                return;

            var transfer = FindNotPlaceHolderTransferViewModel(e.FileNumber);
            if (transfer == null)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                switch (e.Control)
                {
                    case ToxFileControl.Cancel:
                        transfer.CancelTransferByFriend();
                        await _progressUpdater.StopUpdateIfNeeded();
                        return;
                    case ToxFileControl.Pause:
                        transfer.PauseTransferByFriend();
                        await _progressUpdater.StopUpdateIfNeeded();
                        return;
                    case ToxFileControl.Resume:
                        transfer.ResumeTransferByFriend();
                        await _progressUpdater.StartUpdateIfNeeded();
                        return;
                }
            });
        }

        private async void TransferFinishedHandler(object sender, FileTransferManager.TransferFinishedEventArgs e)
        {
            if (FriendNumber != e.FriendNumber)
                return;

            var transfer = FindNotPlaceHolderTransferViewModel(e.FileNumber);
            if (transfer == null)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { transfer.FinishTransfer(); });

            await _progressUpdater.StopUpdateIfNeeded();
        }

        private async void FileDownloadAddedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FriendNumber != FriendNumber)
                return;

            await
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { AddTransfer(e.FileNumber, e.FileName, TransferDirection.Down); });
        }

        private async void FileUploadAddedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FriendNumber != FriendNumber)
                return;

            await
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { AddTransfer(e.FileNumber, e.FileName, TransferDirection.Up); });
        }

        #endregion
    }
}