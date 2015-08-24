using OneTox.ViewModel.Friends;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace OneTox.View.Pages
{
    public sealed partial class ChatPage : NarrowPageBase
    {
        private FriendViewModel _friendViewModel;

        public ChatPage()
        {
            InitializeComponent();

            VisualStateManager.GoToState(ChatBlock, "NarrowState", false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DataContext = _friendViewModel = e.Parameter as FriendViewModel;
        }

        protected override void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 930)
            {
                Frame.Navigate(typeof(MainPage), _friendViewModel);
            }
        }
    }
}
