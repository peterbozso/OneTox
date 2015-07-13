using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel
{
    public class MainPageViewModel
    {
        public MainPageViewModel()
        {
            FriendList = new FriendListViewModel();
        }

        public FriendListViewModel FriendList { get; set; }
    }
}