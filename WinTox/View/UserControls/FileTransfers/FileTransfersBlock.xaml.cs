using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
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

        private async void FileTransferBlockLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as FileTransfersViewModel;
            VisualStateManager.GoToState(this, _viewModel.VisualStates.BlockState.ToString(), true);
            _viewModel.VisualStates.PropertyChanged += VisualStatesPropertyChangedHandler;
            await SetAddDeleteThemeTransitionForTransferRibbons();
        }

        private async Task SetAddDeleteThemeTransitionForTransferRibbons()
        {
            // We need this ugly hack becouse otherwise everytime we navigate to ChatPage
            // we'd see the "Add" animation of every item in the list (and we do not want that).
            await Task.Delay(1);
            TransferRibbons.ItemContainerTransitions = new TransitionCollection {new AddDeleteThemeTransition()};
        }

        private void VisualStatesPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OpenContentGridHeight")
                OpenContentGrid.Height = _viewModel.VisualStates.OpenContentGridHeight;
        }

        private void ShowTransfersIconTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.VisualStates.BlockState = FileTransfersViewModel.VisualStatesViewModel.TransfersBlockState.Open;
        }

        private void HideTransfersIconTapped(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.VisualStates.BlockState =
                FileTransfersViewModel.VisualStatesViewModel.TransfersBlockState.Collapsed;
        }
    }
}