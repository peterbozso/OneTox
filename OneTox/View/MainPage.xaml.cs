using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OneTox.ViewModel;
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

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            var mainViewModel = DataContext as MainViewModel;

            // TODO: Remember which friend we talked to the last time before shutting down the app and resume with selecting him/her.
            // TODO: Handle the case when the user doesn't have any friends yet with a splash screen or something like that!
            if (mainViewModel.FriendList.Friends.Count > 0)
            {
                FriendList.SelectedItem = mainViewModel.FriendList.Friends[0];
            }
        }
    }
}