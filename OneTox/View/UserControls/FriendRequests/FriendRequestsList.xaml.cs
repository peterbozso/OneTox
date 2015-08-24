using OneTox.ViewModel.FriendRequests;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneTox.View.UserControls.FriendRequests
{
    public sealed partial class FriendRequestsList : UserControl
    {
        public FriendRequestsList()
        {
            InitializeComponent();
        }

        private async void FriendRequestsListLoaded(object sender, RoutedEventArgs e)
        {
            var friendRequests = DataContext as FriendRequestsViewModel;

            if (friendRequests.Requests.Count == 0)
            {
                await friendRequests.RestoreData();
            }
        }
    }
}
