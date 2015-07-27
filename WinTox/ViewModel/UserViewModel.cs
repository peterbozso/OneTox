using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using SharpTox.Core;
using WinTox.Model;
using WinTox.Model.Avatars;

namespace WinTox.ViewModel
{
    public class UserViewModel : ViewModelBase, IToxUserViewModel
    {
        public UserViewModel()
        {
            ToxModel.Instance.PropertyChanged += ToxModelPropertyChangedHandler;
            AvatarManager.Instance.UserAvatarChanged += UserAvatarChangedHandler;
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

        public ExtendedToxUserStatus Status
        {
            get
            {
                if (ToxModel.Instance.IsConnected)
                {
                    return (ExtendedToxUserStatus) ToxModel.Instance.Status;
                }

                return ExtendedToxUserStatus.Offline;
            }
        }

        public bool IsConnected
        {
            get { return ToxModel.Instance.IsConnected; }
        }

        private void UserAvatarChangedHandler(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged("Avatar");
        }

        private async void ToxModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    if (e.PropertyName == "IsConnected")
                    {
                        RaisePropertyChanged("Status");
                    }

                    RaisePropertyChanged(e.PropertyName);
                });
        }
    }
}