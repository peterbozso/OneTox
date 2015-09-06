using Windows.UI.Core;
using Windows.UI.Xaml;

namespace OneTox.View.Pages
{
    public sealed partial class ChatPage : NarrowPageBase
    {
        public ChatPage()
        {
            InitializeComponent();

            VisualStateManager.GoToState(ChatBlock, "NarrowState", false);
        }

        protected override void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width >= 930)
            {
                Frame.Navigate(typeof (MainPage));
            }
        }
    }
}