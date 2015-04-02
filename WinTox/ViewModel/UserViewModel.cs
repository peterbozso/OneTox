using SharpTox.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using WinTox.Model;

namespace WinTox.ViewModel
{
    internal class UserViewModel : ViewModelBase
    {
        public UserViewModel()
        {
            _isOnline = ToxSingletonModel.Instance.IsConnected;
            ToxSingletonModel.Instance.OnConnectionStatusChanged += OnConnectionStatusChanged;
        }

        private void OnConnectionStatusChanged(object sender, ToxEventArgs.ConnectionStatusEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                IsOnline = ToxSingletonModel.Instance.IsConnected;
            });
        }

        public string Name
        {
            get { return ToxSingletonModel.Instance.Name; }
        }

        public string StatusMessage
        {
            get { return ToxSingletonModel.Instance.StatusMessage; }
        }

        public ToxUserStatus Status
        {
            get { return ToxSingletonModel.Instance.Status; }
        }

        private bool _isOnline = false;

        public bool IsOnline
        {
            get { return _isOnline; }
            set
            {
                _isOnline = value;
                OnPropertyChanged();
            }
        }
    }
}
