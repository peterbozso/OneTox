using Windows.UI.Core;

namespace OneTox.View.Friends.Pages
{
    public sealed partial class AddFriendPage : NarrowPageBase
    {
        public AddFriendPage()
        {
            InitializeComponent();
        }

        protected override void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 930)
            {
                Frame.Navigate(typeof (MainPage), GetType());
            }
        }
    }
}