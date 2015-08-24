using OneTox.ViewModel.FileTransfers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneTox.View.UserControls.FileTransfers
{
    public sealed partial class FileTransferRibbon : UserControl
    {
        public FileTransferRibbon()
        {
            InitializeComponent();
        }

        private void FileTransferRibbonLoaded(object sender, RoutedEventArgs e)
        {
            var transferViewModel = (OneFileTransferViewModel)DataContext;
            VisualStateManager.GoToState(this, transferViewModel.State.ToString(), true);
        }
    }
}
