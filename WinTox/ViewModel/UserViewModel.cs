using SharpTox.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace WinTox.ViewModel
{
    internal class UserViewModel : ViewModelBase
    {
        public UserViewModel()
        {
            _isOnline = App.ToxModel.IsUserConnected;
            App.ToxModel.UserConnectionStatusChanged += UserConnectionStatusChangedHandler;
        }

        private void UserConnectionStatusChangedHandler(object sender, ToxEventArgs.ConnectionStatusEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                IsOnline = App.ToxModel.IsUserConnected;
            });
        }

        public string Name
        {
            get { return App.ToxModel.UserName; }
        }

        public string StatusMessage
        {
            get { return App.ToxModel.UserStatusMessage; }
        }

        public ToxUserStatus Status
        {
            get { return App.ToxModel.UserStatus; }
        }

        private bool _isOnline;

        public bool IsOnline
        {
            get { return _isOnline; }
            private set
            {
                _isOnline = value;
                OnPropertyChanged();
            }
        }
    }
}
