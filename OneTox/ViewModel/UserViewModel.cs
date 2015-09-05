using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using OneTox.Config;
using OneTox.Helpers;
using OneTox.Model;
using OneTox.Model.Avatars;
using SharpTox.Core;

namespace OneTox.ViewModel
{
    public class UserViewModel : ObservableObject, IToxUserViewModel
    {
        private readonly IToxModel _toxModel;
        private readonly IAvatarManager _avatarManager;

        public UserViewModel(IDataService dataService)
        {
            _toxModel = dataService.ToxModel;
            _avatarManager = dataService.AvatarManager;

            _toxModel.PropertyChanged += ToxModelPropertyChangedHandler;
            _avatarManager.UserAvatarChanged += UserAvatarChangedHandler;
        }

        public ToxId Id => _toxModel.Id;
        public BitmapImage Avatar => _avatarManager.UserAvatar;
        public bool IsConnected => _toxModel.IsConnected;
        public string Name => _toxModel.Name;

        public ExtendedToxUserStatus Status
        {
            get
            {
                if (_toxModel.IsConnected)
                {
                    return (ExtendedToxUserStatus) _toxModel.Status;
                }

                return ExtendedToxUserStatus.Offline;
            }
        }

        public string StatusMessage => _toxModel.StatusMessage;

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

        private void UserAvatarChangedHandler(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged("Avatar");
        }
    }
}