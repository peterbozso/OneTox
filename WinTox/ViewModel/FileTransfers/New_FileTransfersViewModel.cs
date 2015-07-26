using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using WinTox.Common;
using WinTox.Model;

namespace WinTox.ViewModel.FileTransfers
{
    public class New_FileTransfersViewModel
    {
        private readonly int _friendNumber;
        private readonly FileTransfersModel _transfersModel;
        private RelayCommand _sendFilesCommand;

        public New_FileTransfersViewModel(int friendNumber)
        {
            _friendNumber = friendNumber;
            _transfersModel = new FileTransfersModel(friendNumber);
            Transfers = new ObservableCollection<New_OneFileTransferViewModel>();
            VisualStates = new FileTransfersVisualStates();
        }

        public ObservableCollection<New_OneFileTransferViewModel> Transfers { get; private set; }
        public FileTransfersVisualStates VisualStates { get; private set; }

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
                    if (value == _blockState)
                        return;
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
                    if (value.Equals(_openContentGridHeight))
                        return;
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

        #region Send file

        public RelayCommand SendFilesCommand
        {
            get
            {
                return _sendFilesCommand ?? (_sendFilesCommand = new RelayCommand(async () =>
                {
                    var openPicker = new FileOpenPicker();
                    openPicker.FileTypeFilter.Add("*");

                    var files = await openPicker.PickMultipleFilesAsync();
                    if (files.Count == 0)
                        return;

                    foreach (var file in files)
                    {
                        var fileTransferModel = await _transfersModel.SendFile(file);
                        if (fileTransferModel != null)
                        {
                            AddTransfer(fileTransferModel);
                        }
                    }
                }));
            }
        }

        private void AddTransfer(OneFileTransferModel fileTransferModel)
        {
            Transfers.Add(new New_OneFileTransferViewModel(fileTransferModel));

            if (VisualStates.BlockState == FileTransfersVisualStates.TransfersBlockState.Invisible)
            {
                VisualStates.BlockState = FileTransfersVisualStates.TransfersBlockState.Open;
            }

            VisualStates.UpdateOpenContentGridHeight(Transfers.Count);
        }

        #endregion
    }
}