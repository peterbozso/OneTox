using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using OneTox.ViewModel.Messaging;

namespace OneTox.View.UserControls.Messaging.RecentMessages
{
    public sealed partial class RecentMessagesListItem : UserControl
    {
        public RecentMessagesListItem()
        {
            InitializeComponent();
        }

        private void RecentMessageListItemTapped(object sender, TappedRoutedEventArgs e)
        {
           /* 
           (Window.Current.Content as Frame).Navigate(typeof (ChatPage),
                (DataContext as ReceivedMessageViewModel).Sender);
            */
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            CapturePointer(e.Pointer);
            VisualStateManager.GoToState(this, "PointerDown", true);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerUp", true);
            ReleasePointerCapture(e.Pointer);
        }
    }
}