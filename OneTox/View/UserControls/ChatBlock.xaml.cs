using Windows.UI.Xaml.Controls;
using OneTox.ViewModel.Friends;

namespace OneTox.View.UserControls
{
    public sealed partial class ChatBlock : UserControl
    {
        private FriendViewModel _friendViewModel;

        public ChatBlock()
        {
            InitializeComponent();
        }

        public void SetDataContext(FriendViewModel friendViewModel)
        {
            DataContext = friendViewModel;
            _friendViewModel = friendViewModel;
        }
    }
}