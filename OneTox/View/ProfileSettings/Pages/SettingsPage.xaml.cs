using Windows.UI.Core;
using Windows.UI.Xaml;

namespace OneTox.View.ProfileSettings.Pages
{
    public sealed partial class SettingsPage : NarrowPageBase
    {
        public SettingsPage()
        {
            InitializeComponent();

            VisualStateManager.GoToState(ProfileSettingsBlock, "NarrowState", false);
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