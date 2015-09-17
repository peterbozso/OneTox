using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneTox.View
{
    public sealed partial class NavigationBar : UserControl
    {
        public NavigationBar()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler AddFriendClick;
        public event RoutedEventHandler SettingsClick;

        private void AddFriendButtonClick(object sender, RoutedEventArgs e)
        {
            AddFriendClick?.Invoke(this, e);
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            SettingsClick?.Invoke(this, e);
        }
    }
}