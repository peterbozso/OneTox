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
            _isOnline = App.ToxModel.IsConnected;
            App.ToxModel.UserConnectionStatusChanged += UserConnectionStatusChangedHandler;
        }

        public ToxId Id
        {
            get { return App.ToxModel.Id; }
        }

        public string Name
        {
            get { return App.ToxModel.Name; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (value == String.Empty || lengthInBytes > ToxConstants.MaxNameLength ||
                    App.ToxModel.Name == value)
                    return;
                App.ToxModel.Name = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get { return App.ToxModel.StatusMessage; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (lengthInBytes > ToxConstants.MaxStatusMessageLength)
                    return;
                App.ToxModel.StatusMessage = value;
                OnPropertyChanged();
            }
        }

        public ToxUserStatus Status
        {
            get { return App.ToxModel.Status; }
            set
            {
                App.ToxModel.Status = value;
                OnPropertyChanged();
            }
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
                () => { IsOnline = App.ToxModel.IsConnected; });
        }
    }
}