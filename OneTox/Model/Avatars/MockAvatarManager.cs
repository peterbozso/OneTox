using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace OneTox.Model.Avatars
{
    internal class MockAvatarManager : IAvatarManager
    {
        public Dictionary<int, BitmapImage> FriendAvatars => new Dictionary<int, BitmapImage>();
        public BitmapImage UserAvatar => new BitmapImage(new Uri("ms-appx:///Assets/default-profile-picture.png"));
        public bool IsUserAvatarSet => false;

        public Task LoadAvatars()
        {
            return Task.FromResult(new object());
        }

        public Task ChangeUserAvatar(StorageFile file)
        {
            return Task.FromResult(new object());
        }

        public Task RemoveUserAvatar()
        {
            return Task.FromResult(new object());
        }

        public event EventHandler<int> FriendAvatarChanged;
        public event EventHandler UserAvatarChanged;
        public event EventHandler IsUserAvatarSetChanged;
    }
}