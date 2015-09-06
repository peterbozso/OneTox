using System;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using GalaSoft.MvvmLight.Threading;
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

        private async void NameTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var nameTextBox = sender as TextBox;
            if (nameTextBox.Text == string.Empty)
            {
                nameTextBox.Text = _viewModel.Name; // We reset the name if the user just deleted it.
            }
            else
            {
                await _viewModel.SaveDataAsync();
            }
        }

        private async void StatusComboBoxLostFocus(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveDataAsync();
        }

        private async void StatusMessageTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveDataAsync();
        }

        private async void UserAvatarTapped(object sender, TappedRoutedEventArgs e)
        {
            await _viewModel.ChangeAvatar();
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
                        state =>
                            DispatcherHelper.CheckBeginInvokeOnUI(
                                () => { ClipboardCopyConfirmIcon.Visibility = Visibility.Collapsed; }),
                        null, 3000, Timeout.Infinite);
            }
            else
            {
                _copyClipboardTimer.Change(3000, Timeout.Infinite);
            }
        }

        #endregion Tox ID
    }
}