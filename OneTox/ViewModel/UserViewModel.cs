using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using OneTox.Helpers;
using OneTox.Model;
using OneTox.Model.Avatars;
using SharpTox.Core;

namespace OneTox.ViewModel
{
    public class UserViewModel : ObservableObject, IToxUserViewModel
    {
        public UserViewModel()
        {
            ToxModel.Instance.PropertyChanged += ToxModelPropertyChangedHandler;
            AvatarManager.Instance.UserAvatarChanged += UserAvatarChangedHandler;
        }

        public ToxId Id => ToxModel.Instance.Id;
        public BitmapImage Avatar => AvatarManager.Instance.UserAvatar;
        public string Name => ToxModel.Instance.Name;
        public string StatusMessage => ToxModel.Instance.StatusMessage;
        public bool IsConnected => ToxModel.Instance.IsConnected;

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