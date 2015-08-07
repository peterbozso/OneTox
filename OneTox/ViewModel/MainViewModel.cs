using OneTox.ViewModel.Friends;

namespace OneTox.ViewModel
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            FriendList = new FriendListViewModel();
        }

        public FriendListViewModel FriendList { get; private set; }
    }
}