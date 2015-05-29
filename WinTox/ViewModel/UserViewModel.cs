using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel
{
    public class UserViewModel : ViewModelBase, IToxUserViewModel
    {
        public UserViewModel()
        {
            ToxModel.Instance.PropertyChanged += ToxModelPropertyChangedHandler;
            AvatarManager.Instance.PropertyChanged += AvatarManagerPropertyChangedHandler;
        }

        public ToxId Id
        {
            get { return ToxModel.Instance.Id; }
        }

        public BitmapImage Avatar
        {
            get { return AvatarManager.Instance.UserAvatar; }
        }

        public string Name
        {
            get { return ToxModel.Instance.Name; }
        }

        public string StatusMessage
        {
            get { return ToxModel.Instance.StatusMessage; }
        }

        public ToxUserStatus Status
        {
            get { return ToxModel.Instance.Status; }
        }

        public bool IsConnected
        {
            get { return ToxModel.Instance.IsConnected; }
        }

        private void AvatarManagerPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UserAvatar")
                RaisePropertyChanged("Avatar");
        }

        private void ToxModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { RaisePropertyChanged(e.PropertyName); });
        }
    }
}