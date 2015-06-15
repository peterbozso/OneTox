using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinTox.ViewModel.Messaging;

namespace WinTox.View.UserControls.Messaging.RecentMessages
{
    public sealed partial class RecentMessagesListItem : UserControl
    {
        public RecentMessagesListItem()
        {
            InitializeComponent();
        }

        private void RecentMessageListItemTapped(object sender, TappedRoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof (ChatPage), (DataContext as ReceivedMessageViewModel).Sender);
        }
    }
}