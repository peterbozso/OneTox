using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinTox.ViewModel.FileTransfers;

namespace WinTox.View.UserControls.FileTransfers
{
    public sealed partial class FileTransfersBlock : UserControl
    {
        private FileTransfersViewModel _viewModel;

        public FileTransfersBlock()
        {
            InitializeComponent();
        }

        private void FileTransferBlockLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as FileTransfersViewModel;
            VisualStateManager.GoToState(this, _viewModel.VisualStates.BlockState.ToString(), true);
            _viewModel.VisualStates.PropertyChanged += VisualStatesPropertyChangedHandler;
        }

        private void VisualStatesPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OpenContentGridHeight")
                OpenContentGrid.Height = _viewModel.VisualStates.OpenContentGridHeight;
        }

        private void ShowArrowTextBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.VisualStates.BlockState = FileTransfersViewModel.VisualStatesViewModel.TransfersBlockState.Open;
        }

        private void HideArrowTextBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.VisualStates.BlockState =
                FileTransfersViewModel.VisualStatesViewModel.TransfersBlockState.Collapsed;
        }
    }
}