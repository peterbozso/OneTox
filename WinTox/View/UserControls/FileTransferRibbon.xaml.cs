using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTox.ViewModel.FileTransfer;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WinTox.View.UserControls
{
    public sealed partial class FileTransferRibbon : UserControl
    {
        public FileTransferRibbon()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var transfer = (OneFileTransferViewModel) DataContext;
            if (transfer.Phase == FileTransferPhase.BeforeUpload)
                VisualStateManager.GoToState(this, "BeforeUpload", true);
            else if (transfer.Phase == FileTransferPhase.BeforeDownload)
                VisualStateManager.GoToState(this, "BeforeDownload", true);
        }
    }
}