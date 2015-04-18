using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    internal class UserViewModel : ViewModelBase, IToxUserViewModel
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