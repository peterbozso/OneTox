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
        private readonly int _friendNumber;
        private DispatcherTimer _progressDispatcherTimer;

        public FileTransfersViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            Transfers = new ObservableCollection<OneFileTransferViewModel>();
            FileTransferManager.Instance.FileControlReceived += FileControlReceivedHandler;
            FileTransferManager.Instance.TransferFinished += TransferFinishedHandler;
            FileTransferManager.Instance.FileSendRequestReceived += FileSendRequestReceivedHandler;
            SetupProgressDispatcherTimer();
            VisualStates = new VisualStatesViewModel {BlockState = VisualStatesViewModel.TransfersBlockState.Invisible};
        }

        public ObservableCollection<OneFileTransferViewModel> Transfers { get; private set; }

        #region Helper methods

        private OneFileTransferViewModel FindNotPlaceHolderTransferViewModel(int fileNumber)
        {
            return Transfers.FirstOrDefault(transfer => transfer.FileNumber == fileNumber && transfer.IsNotPlaceholder);
            // There can be multiple transfers with the same file number, but there's always only one that's not a placeholder.
        }

        private void AddTransfer(int fileNumber, string fileName, FileTransferState direction)
        {
            Transfers.Add(new OneFileTransferViewModel(this, fileNumber, fileName, direction));

            if (VisualStates.BlockState == VisualStatesViewModel.TransfersBlockState.Invisible)
                VisualStates.BlockState = VisualStatesViewModel.TransfersBlockState.Open;

            VisualStates.UpdateOpenContentGridHeight(Transfers.Count);
        }

        private void RemoveTransfer(OneFileTransferViewModel transferViewModel)
        {
            Transfers.Remove(transferViewModel);

            if (Transfers.Count == 0)
                VisualStates.BlockState = VisualStatesViewModel.TransfersBlockState.Invisible;

            VisualStates.UpdateOpenContentGridHeight(Transfers.Count);
        }

        #endregion

        #region Helper methods for DispatcherTimer

        private void SetupProgressDispatcherTimer()
        {
            _progressDispatcherTimer = new DispatcherTimer();
            _progressDispatcherTimer.Tick += (s, e) =>
            {
                var progresses = FileTransferManager.Instance.GetTransferProgressesOfFriend(_friendNumber);
                foreach (var progress in progresses)
                {
                    var transfer = FindNotPlaceHolderTransferViewModel(progress.Key);
                    if (transfer == null)
                        continue;

                    transfer.Progress = progress.Value;
                }
            };
            _progressDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
        }

        private async Task StartTimerIfNeeded()
        {
            if (GetActiveTransfersCount() > 0)
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { _progressDispatcherTimer.Start(); });
            }
        }

        private async Task StopTimerIfNeeded()
        {
            if (GetActiveTransfersCount() == 0)
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { _progressDispatcherTimer.Stop(); });
            }
        }

        private int GetActiveTransfersCount()
        {
            return Transfers.Count(transfer => transfer.IsNotPlaceholder &&
                                               transfer.State != FileTransferState.PausedByFriend &&
                                               transfer.State != FileTransferState.PausedByUser);
        }

        #endregion

        #region Actions coming from the View

        public async Task SendFile(StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            int fileNumber;

            var successfulSend = FileTransferManager.Instance.SendFile(_friendNumber, stream, file.Name, out fileNumber);

            if (successfulSend)
            {
                AddTransfer(fileNumber, file.Name, FileTransferState.Uploading);
            }
            else
            {
                stream.Dispose();
            }
        }

        public async Task AcceptTransferByUser(int fileNumber, Stream saveStream)
        {
            FileTransferManager.Instance.ReceiveFile(_friendNumber, fileNumber, saveStream);
            await StartTimerIfNeeded();
        }

        public async Task CancelTransferByUser(OneFileTransferViewModel transferViewModel)
        {
            FileTransferManager.Instance.CancelTransfer(_friendNumber, transferViewModel.FileNumber);
            RemoveTransfer(transferViewModel);
            await StopTimerIfNeeded();
        }

        public async Task PauseTransferByUser(int fileNumber)
        {
            FileTransferManager.Instance.PauseTransfer(_friendNumber, fileNumber);
            await StopTimerIfNeeded();
        }

        public async Task ResumeTransferByUser(int fileNumber)
        {
            FileTransferManager.Instance.ResumeTransfer(_friendNumber, fileNumber);
            await StartTimerIfNeeded();
        }

        #endregion

        #region Visual states for the View

        public class VisualStatesViewModel : ViewModelBase
        {
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

            public TransfersBlockState BlockState
            {
                get { return _blockState; }
                set
                {
                    _blockState = value;
                    RaisePropertyChanged();
                }
            }

            public double OpenContentGridHeight
            {
                get { return _openContentGridHeight; }
                private set
                {
                    _openContentGridHeight = value;
                    RaisePropertyChanged();
                }
            }

            public void UpdateOpenContentGridHeight(int itemsCount)
            {
                var itemsToDisplay = itemsCount > 4 ? 4 : itemsCount;
                    // We don't show more than 4 items in the list at once.
                OpenContentGridHeight = itemsToDisplay*KFileTransferRibbonHeight + KHideArrowTextBlockHeight;
            }
        }

        public VisualStatesViewModel VisualStates { get; private set; }

        #endregion

        #region Actions coming from the Model

        private async void FileControlReceivedHandler(int friendNumber, int fileNumber, ToxFileControl fileControl)
        {
            if (_friendNumber != friendNumber)
                return;

            var transfer = FindNotPlaceHolderTransferViewModel(fileNumber);
            if (transfer == null)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                switch (fileControl)
                {
                    case ToxFileControl.Cancel:
                        transfer.CancelTransferByFriend();
                        await StopTimerIfNeeded();
                        return;
                    case ToxFileControl.Pause:
                        transfer.PauseTransferByFriend();
                        await StopTimerIfNeeded();
                        return;
                    case ToxFileControl.Resume:
                        transfer.ResumeTransferByFriend();
                        await StartTimerIfNeeded();
                        return;
                }
            });
        }

        private async void TransferFinishedHandler(int friendNumber, int fileNumber)
        {
            if (_friendNumber != friendNumber)
                return;

            var transfer = FindNotPlaceHolderTransferViewModel(fileNumber);
            if (transfer == null)
                return;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { transfer.FinishTransfer(); });

            await StopTimerIfNeeded();
        }

        private async void FileSendRequestReceivedHandler(object sender, ToxEventArgs.FileSendRequestEventArgs e)
        {
            if (e.FriendNumber != _friendNumber)
                return;

            await
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { AddTransfer(e.FileNumber, e.FileName, FileTransferState.Downloading); });
        }

        #endregion
    }
}