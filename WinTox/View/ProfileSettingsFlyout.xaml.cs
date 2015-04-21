using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WinTox.View
{
    public sealed partial class ProfileSettingsFlyout : SettingsFlyout
    {
        public ProfileSettingsFlyout()
        {
            InitializeComponent();
            DataContext = App.UserViewModel;
        }

        private void NameTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == String.Empty)
                textBox.Text = App.UserViewModel.Name;
        }
    }
}