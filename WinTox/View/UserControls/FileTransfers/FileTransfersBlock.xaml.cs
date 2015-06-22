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

            HidePlaceholderStoryboard.Completed += HidePlaceholderStoryboardCompleted;
            HideTransfersStoryboard.Completed += HideTransfersStoryboardCompleted;
        }

        private void FileTransferBlockLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as FileTransfersViewModel;
            VisualStateManager.GoToState(this, _viewModel.VisualStates.BlockState.ToString(), true);
        }

        #region Show arrow tap

        private void ShowArrowTextBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            HidePlaceholderStoryboard.Begin();
        }

        private void HidePlaceholderStoryboardCompleted(object sender, object e)
        {
            _viewModel.VisualStates.BlockState = FileTransfersViewModel.VisualStatesViewModel.TransfersBlockState.Open;
            ShowTransfersStoryboard.Begin();
        }

        #endregion

        #region Hide arrow tap

        private void HideArrowTextBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            HideTransfersStoryboard.Begin();
        }

        private void HideTransfersStoryboardCompleted(object sender, object e)
        {
            _viewModel.VisualStates.BlockState =
                FileTransfersViewModel.VisualStatesViewModel.TransfersBlockState.Collapsed;
            ShowPlaceholderStoryboard.Begin();
        }

        #endregion
    }
}