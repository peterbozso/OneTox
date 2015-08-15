using System;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using OneTox.ViewModel.ProfileSettings;
using SharpTox.Core;

namespace OneTox.View.UserControls.ProfileSettings
{
    public sealed partial class ProfileSettingsBlock : UserControl
    {
        private readonly ProfileSettingsViewModel _viewModel;
        private Timer _copyClipboardTimer;

        public ProfileSettingsBlock()
        {
            InitializeComponent();
            _viewModel = DataContext as ProfileSettingsViewModel;
            StatusComboBox.ItemsSource = Enum.GetValues(typeof (ToxUserStatus)).Cast<ToxUserStatus>();
        }

        private async void UserAvatarTapped(object sender, TappedRoutedEventArgs e)
        {
            await _viewModel.ChangeAvatar();
        }

        private async void StatusComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await _viewModel.SaveDataAsync();
        }

        private async void NameTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var nameTextBox = sender as TextBox;
            if (nameTextBox.Text == string.Empty)
            {
                nameTextBox.Text = _viewModel.Name;
            }
            else
            {
                await _viewModel.SaveDataAsync();
            }
        }

        private async void StatusMessageTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveDataAsync();
        }

        #region Tox ID

        private void CopyButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.CopyToxIdToClipboard();
            ShowCopyConfirm();
        }

        private void ShowCopyConfirm()
        {
            ClipboardCopyConfirmIcon.Visibility = Visibility.Visible;

            if (_copyClipboardTimer == null)
            {
                _copyClipboardTimer =
                    new Timer(
                        async state =>
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                () => { ClipboardCopyConfirmIcon.Visibility = Visibility.Collapsed; }),
                        null, 3000, Timeout.Infinite);
            }
            else
            {
                _copyClipboardTimer.Change(3000, Timeout.Infinite);
            }
        }

        #endregion
    }
}