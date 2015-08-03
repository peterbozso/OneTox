using System;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using OneTox.ViewModel.ProfileSettings;
using SharpTox.Core;

namespace OneTox.View
{
    public sealed partial class ProfileSettingsFlyout : SettingsFlyout
    {
        private readonly ProfileSettingsViewModel _viewModel;
        private Timer _copyClipboardTimer;

        public ProfileSettingsFlyout()
        {
            InitializeComponent();
            _viewModel = DataContext as ProfileSettingsViewModel;
            StatusComboBox.ItemsSource = Enum.GetValues(typeof (ToxUserStatus)).Cast<ToxUserStatus>();
        }

        private void NameTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var nameTextBox = sender as TextBox;
            if (nameTextBox.Text == string.Empty)
                nameTextBox.Text = _viewModel.Name;
        }

        private void CopyButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.CopyToxIdToClipboard();
            ShowCopyConfirm();
        }

        private void ShowCopyConfirm()
        {
            ClipboardCopyConfirm.Visibility = Visibility.Visible;

            if (_copyClipboardTimer == null)
            {
                _copyClipboardTimer =
                    new Timer(
                        async state =>
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                () => { ClipboardCopyConfirm.Visibility = Visibility.Collapsed; }),
                        null, 3000, Timeout.Infinite);
            }
            else
            {
                _copyClipboardTimer.Change(3000, Timeout.Infinite);
            }
        }

        private void QrCodeButtonClick(object sender, RoutedEventArgs e)
        {
            QrCodeImage.Source = _viewModel.GetQrCodeForToxId();
        }

        private async void ProfileSettingsFlyoutLostFocus(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveDataAsync();
        }

        private async void UserAvatarTapped(object sender, TappedRoutedEventArgs e)
        {
            await _viewModel.ChangeAvatar();

            // Show the settings again when we return, in case the user want to do more than just changing his/her avatar.
            App.ShowProfileSettingsFlyout();
        }

        private void NameTextBoxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && NameTextBox.Text != string.Empty)
            {
                if (e.KeyStatus.RepeatCount != 1) // See MessageInputKeyDown()!
                    return;

                _viewModel.Name = NameTextBox.Text;
                e.Handled = true;
            }
        }

        private void StatusMessageTextBoxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && StatusMessageTextBox.Text != string.Empty)
            {
                if (e.KeyStatus.RepeatCount != 1) // See MessageInputKeyDown()!
                    return;

                _viewModel.StatusMessage = StatusMessageTextBox.Text;
                e.Handled = true;
            }
        }
    }
}