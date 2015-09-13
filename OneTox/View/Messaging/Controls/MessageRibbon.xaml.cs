using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OneTox.ViewModel.Messaging;

namespace OneTox.View.Messaging.Controls
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