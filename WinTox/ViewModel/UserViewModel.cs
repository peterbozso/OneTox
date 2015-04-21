using System;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    public class UserViewModel : ViewModelBase, IToxUserViewModel
    {
        private bool _isOnline;

        public UserViewModel()
        {
            _isOnline = App.ToxModel.IsUserConnected;
            App.ToxModel.UserConnectionStatusChanged += UserConnectionStatusChangedHandler;
        }

        public string Name
        {
            get { return App.ToxModel.UserName; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (value == String.Empty || lengthInBytes > ToxConstants.MaxNameLength ||
                    App.ToxModel.UserName == value)
                    return;
                App.ToxModel.UserName = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get { return App.ToxModel.UserStatusMessage; }
        }

        public ToxUserStatus Status
        {
            get { return App.ToxModel.UserStatus; }
        }

        public bool IsOnline
        {
            get { return _isOnline; }
            private set
            {
                _isOnline = value;
                OnPropertyChanged();
            }
        }

        private void UserConnectionStatusChangedHandler(object sender, ToxEventArgs.ConnectionStatusEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { IsOnline = App.ToxModel.IsUserConnected; });
        }
    }
}