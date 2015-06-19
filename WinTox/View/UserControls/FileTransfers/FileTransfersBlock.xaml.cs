using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace WinTox.View.UserControls.FileTransfers
{
    public sealed partial class FileTransfersBlock : UserControl
    {
        public FileTransfersBlock()
        {
            InitializeComponent();
            VisualStateManager.GoToState(this, "Invisible", true);
        }

        private void ShowArrowTextBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Open", true);
        }

        private void HideArrowTextBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Collapsed", true);
        }
    }
}