using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SharpTox.Core;
using WinTox.Common;

namespace WinTox.ViewModel {
    class AddFriendFlyoutViewModel : INotifyPropertyChanged {
        #region Properties

        private bool _isFlyoutOpen;

        public bool IsFlyoutOpen {
            get { return _isFlyoutOpen; }
            set {
                _isFlyoutOpen = value;
                OnPropertyChanged();
            }
        }

        private RelayCommand _addFriendCommand;

        public RelayCommand AddFriendCommand {
            get {
                return _addFriendCommand
                    ?? (_addFriendCommand = new RelayCommand(
                    (object parameter) => {
                        var flyoutContent = (StackPanel)parameter;

                        var friendIdTextBox = (TextBox)flyoutContent.FindName("FriendID");
                        var friendId = friendIdTextBox.Text;
                        if (friendId == String.Empty) {
                            friendIdTextBox.Focus(FocusState.Programmatic);
                            return;
                        }

                        var invitationMessage = ((TextBox)flyoutContent.FindName("InvitationMessage")).Text;
                        if (invitationMessage == String.Empty)
                            invitationMessage = "Hello! I'd like to add you to my friends list.";

                        ToxErrorFriendAdd error;
                        ToxViewModel.Instance.AddFriend(new ToxId(friendId), invitationMessage, out error);
                        // TODO: Handle errors!!!

                        IsFlyoutOpen = false;
                    }));
            }
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region DependencyProperty

        // From: https://marcominerva.wordpress.com/2013/07/30/using-windows-8-1-flyout-xaml-control-with-mvvm/

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.RegisterAttached("IsOpen", typeof(bool),
                typeof(AddFriendFlyoutViewModel), new PropertyMetadata(false, OnIsOpenPropertyChanged));

        public static readonly DependencyProperty ParentProperty =
            DependencyProperty.RegisterAttached("Parent", typeof(Button),
                typeof(AddFriendFlyoutViewModel), new PropertyMetadata(null, OnParentPropertyChanged));

        public static void SetIsOpen(DependencyObject d, bool value) {
            d.SetValue(IsOpenProperty, value);
        }

        public static bool GetIsOpen(DependencyObject d) {
            return (bool)d.GetValue(IsOpenProperty);
        }

        public static void SetParent(DependencyObject d, Button value) {
            d.SetValue(ParentProperty, value);
        }

        public static Button GetParent(DependencyObject d) {
            return (Button)d.GetValue(ParentProperty);
        }

        private static void OnParentPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e) {
            var flyout = d as Flyout;
            if (flyout != null) {
                flyout.Opening += (s, args) => {
                    flyout.SetValue(IsOpenProperty, true);
                };

                flyout.Closed += (s, args) => {
                    flyout.SetValue(IsOpenProperty, false);
                };
            }
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e) {
            var flyout = d as Flyout;
            var parent = (Button)d.GetValue(ParentProperty);

            if (flyout != null && parent != null) {
                var newValue = (bool)e.NewValue;

                if (newValue)
                    flyout.ShowAt(parent);
                else
                    flyout.Hide();
            }
        }

        #endregion  
    }
}
