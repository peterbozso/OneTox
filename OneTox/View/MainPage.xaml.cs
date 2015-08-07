using Windows.UI.Xaml.Controls;
using OneTox.ViewModel.Friends;

namespace OneTox.View
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void FriendListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChatBlock.SetDataContext(FriendList.SelectedItem as FriendViewModel);
        }
    }
}