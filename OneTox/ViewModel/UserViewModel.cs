using System;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using OneTox.Config;
using OneTox.Model.Avatars;
using OneTox.Model.Tox;
using SharpTox.Core;

namespace OneTox.ViewModel
{
    public class UserViewModel : ViewModelBase, IToxUserViewModel
    {
        private readonly IAvatarManager _avatarManager;
        private readonly IToxModel _toxModel;

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

        private void ToxModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
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