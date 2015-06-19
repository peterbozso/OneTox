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

        private void FileTransferBlockUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as FileTransfersViewModel;
            VisualStateManager.GoToState(this, _viewModel.TransfersBlockState.ToString(), true);
        }

        private void ShowArrowTextBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.TransfersBlockState = FileTransfersViewModel.BlockState.Open;
        }

        private void HideArrowTextBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.TransfersBlockState = FileTransfersViewModel.BlockState.Collapsed;
        }
    }
}