using System.Collections.Specialized;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using OneTox.ViewModel.FileTransfers;

namespace OneTox.View.FileTransfers.Controls
{
    public sealed partial class FileTransfersBlock : UserControl
    {
        private FileTransfersViewModel _fileTransfersViewModel;

        public FileTransfersBlock()
        {
            InitializeComponent();
        }

        private void FileTransferBlockDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null)
                return;

            _fileTransfersViewModel = DataContext as FileTransfersViewModel;
            _fileTransfersViewModel.VisualStates.PropertyChanged += VisualStatesPropertyChangedHandler;

            VisualStateManager.GoToState(this, _fileTransfersViewModel.VisualStates.BlockState.ToString(), false);
            _fileTransfersViewModel.Transfers.CollectionChanged += TransfersCollectionChangedHandler;
        }

        private void HideTransfersIconTapped(object sender, TappedRoutedEventArgs e)
        {
            _fileTransfersViewModel.VisualStates.BlockState =
                FileTransfersViewModel.FileTransfersVisualStates.TransfersBlockState.Collapsed;
        }

        private void ScrollTransferRibbonsToBottom()
        {
            TransferRibbonsScrollViewer.UpdateLayout();
            TransferRibbonsScrollViewer.ChangeView(null, double.MaxValue, null, true);
        }

        private void ShowTransfersIconTapped(object sender, TappedRoutedEventArgs e)
        {
            _fileTransfersViewModel.VisualStates.BlockState =
                FileTransfersViewModel.FileTransfersVisualStates.TransfersBlockState.Open;
        }

        private void TransfersCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ScrollTransferRibbonsToBottom();
            }
        }

        private void VisualStatesPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OpenContentGridHeight")
            {
                AdjustOpenGridAnimationHeights();
            }
        }

        private void FileTransfersBlockLoaded(object sender, RoutedEventArgs e)
        {
            AdjustOpenGridAnimationHeights();
        }

        private void AdjustOpenGridAnimationHeights()
        {
            // Storyboards don't really like data binded From and To values, so we have to do it this way, updating these manually.
            var newHeight = _fileTransfersViewModel.VisualStates.OpenContentGridHeight;
            ShowOpenContentGridAnimationEnd.Value = newHeight;
            OpenContentGridHeight.Value = newHeight;
        }
    }
}