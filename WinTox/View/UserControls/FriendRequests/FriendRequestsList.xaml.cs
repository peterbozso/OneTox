using Windows.UI.Xaml.Controls;
using WinTox.ViewModel.FriendRequests;

namespace WinTox.View.UserControls.FriendRequests
{
    public sealed partial class FriendRequestsList : UserControl
    {
        public FriendRequestsList()
        {
            InitializeComponent();

            ContentControl.ItemsSource = FriendRequestsViewModel.Instance.FriendRequests;
        }
    }
}