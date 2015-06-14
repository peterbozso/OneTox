using Windows.UI.Xaml.Controls;
using WinTox.ViewModel.Messaging;

namespace WinTox.View.UserControls
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