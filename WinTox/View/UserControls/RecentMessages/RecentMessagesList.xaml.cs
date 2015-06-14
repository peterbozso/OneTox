using Windows.UI.Xaml.Controls;
using WinTox.ViewModel.Messaging;

namespace WinTox.View.UserControls.RecentMessages
{
    public sealed partial class RecentMessagesList : UserControl
    {
        public RecentMessagesList()
        {
            InitializeComponent();
            DataContext = RecentMessagesViewModel.Instace;
        }
    }
}