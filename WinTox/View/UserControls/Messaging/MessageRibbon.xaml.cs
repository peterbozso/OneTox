using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinTox.ViewModel.Messaging;

namespace WinTox.View.UserControls.Messaging
{
    public sealed partial class MessageRibbon : UserControl
    {
        public MessageRibbon()
        {
            InitializeComponent();
        }

        private void MessageRibbonLoaded(object sender, RoutedEventArgs e)
        {
            var messageViewModel = (ToxMessageViewModelBase) DataContext;
            VisualStateManager.GoToState(this, messageViewModel.State.ToString(), true);
        }
    }
}