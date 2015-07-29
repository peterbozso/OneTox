using Windows.UI.Xaml.Controls;
using OneTox.ViewModel.Messaging.RecentMessages;

namespace OneTox.View.UserControls.Messaging.RecentMessages
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