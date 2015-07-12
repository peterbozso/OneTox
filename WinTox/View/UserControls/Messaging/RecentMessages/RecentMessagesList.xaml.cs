using Windows.UI.Xaml.Controls;
using WinTox.ViewModel.Messaging.RecentMessages;

namespace WinTox.View.UserControls.Messaging.RecentMessages
{
    public sealed partial class RecentMessagesList : UserControl
    {
        public RecentMessagesList()
        {
            InitializeComponent();
            ContentControl.ItemsSource = RecentMessagesGlobalViewModel.Instace.RecentMessages;
        }
    }
}