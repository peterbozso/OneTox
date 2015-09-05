using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace OneTox.Model.Avatars
{
    public interface IAvatarManager
    {
        Dictionary<int, BitmapImage> FriendAvatars { get; }

        BitmapImage UserAvatar { get; }

        bool IsUserAvatarSet { get; }

        Task LoadAvatars();

        Task ChangeUserAvatar(StorageFile file);

        Task RemoveUserAvatar();

        event EventHandler<int> FriendAvatarChanged;

        event EventHandler UserAvatarChanged;

        event EventHandler IsUserAvatarSetChanged;
    }
}