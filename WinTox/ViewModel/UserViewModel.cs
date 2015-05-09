using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    public class UserViewModel : ViewModelBase, IToxUserViewModel
    {
        public UserViewModel()
        {
            App.ToxModel.PropertyChanged += ToxModelPropertyChangedHandler;
        }

        public ToxId Id
        {
            get { return App.ToxModel.Id; }
        }

        public string Name
        {
            get { return App.ToxModel.Name; }
        }

        public string StatusMessage
        {
            get { return App.ToxModel.StatusMessage; }
        }

        public ToxUserStatus Status
        {
            get { return App.ToxModel.Status; }
        }

        public bool IsConnected
        {
            get { return App.ToxModel.IsConnected; }
        }

        private void ToxModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { RaisePropertyChanged(e.PropertyName); });
        }
    }
}