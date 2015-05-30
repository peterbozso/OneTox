using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using SharpTox.Core;

namespace WinTox.Model
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class AvatarManager
    {
        // See: https://github.com/irungentoo/Tox_Client_Guidelines/blob/master/Important/Avatars.md
        private const int KMaxPictureSize = 1 << 16;
        private static AvatarManager _instance;
        private StorageFolder _avatarsFolder;
        private bool _isUserAvatarSet;
        private BitmapImage _userAvatar;

        private AvatarManager()
        {
            ResetUserAvatar();
            ToxModel.Instance.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
            FriendAvatars = new Dictionary<int, BitmapImage>();
        }

        public static AvatarManager Instance
        {
            get { return _instance ?? (_instance = new AvatarManager()); }
        }

        public bool IsUserAvatarSet
        {
            get { return _isUserAvatarSet; }
            private set
            {
                _isUserAvatarSet = value;
                if (IsUserAvatarSetChanged != null)
                    IsUserAvatarSetChanged(this, new EventArgs());
            }
        }

        public BitmapImage UserAvatar
        {
            get { return _userAvatar; }
            private set
            {
                _userAvatar = value;
                if (UserAvatarChanged != null)
                    UserAvatarChanged(this, new EventArgs());
            }
        }

        public Dictionary<int, BitmapImage> FriendAvatars { get; private set; }

        public async Task ChangeUserAvatar(StorageFile file)
        {
            await SetUserAvatar(file);
            var copy = await file.CopyAsync(_avatarsFolder);
            await copy.RenameAsync(ToxModel.Instance.Id.PublicKey + ".png", NameCollisionOption.ReplaceExisting);
            await BroadcastUserAvatarOnSet(copy);
        }

        // We presume that this is called before any other function that use _avatarsFolder.
        public async Task LoadAvatars()
        {
            _avatarsFolder =
                await ApplicationData.Current.RoamingFolder.CreateFolderAsync(
                    "avatars", CreationCollisionOption.OpenIfExists);

            await LoadUserAvatar();
            await LoadFriendAvatars();
        }

        private async Task LoadUserAvatar()
        {
            var file = await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.Id.PublicKey + ".png");
            if (file != null)
                await SetUserAvatar(file as StorageFile);
        }

        private async Task LoadFriendAvatars()
        {
            foreach (var friendNumber in ToxModel.Instance.Friends)
            {
                var publicKey = ToxModel.Instance.GetFriendPublicKey(friendNumber);
                var file = await _avatarsFolder.TryGetItemAsync(publicKey + ".png") as StorageFile;
                if (file == null)
                    continue;
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var friendAvatar = new BitmapImage();
                    await friendAvatar.SetSourceAsync(stream);
                    FriendAvatars[friendNumber] = friendAvatar;
                    if (FriendAvatarChanged != null)
                        FriendAvatarChanged(this, friendNumber);
                }
            }
        }

        private async Task SetUserAvatar(StorageFile file)
        {
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                if (stream.AsStream().Length > KMaxPictureSize)
                    throw new ArgumentOutOfRangeException();
                UserAvatar.SetSource(stream);
                IsUserAvatarSet = true;
            }
        }

        private async Task BroadcastUserAvatarOnSet(StorageFile file)
        {
            foreach (var friend in ToxModel.Instance.Friends)
            {
                await FileTransferManager.Instance.SendAvatar(friend, file);
            }
        }

        private async void FriendConnectionStatusChangedHandler(object sender,
            ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (ToxModel.Instance.IsFriendOnline(e.FriendNumber))
            {
                var file = await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.Id.PublicKey + ".png");
                if (file != null)
                {
                    await FileTransferManager.Instance.SendAvatar(e.FriendNumber, (StorageFile) file);
                }
                else // We have no saved avatar for the user: we have no avatar set.
                {
                    FileTransferManager.Instance.SendNullAvatar(e.FriendNumber);
                }
            }
        }

        /// <summary>
        ///     Removes the current avatar of the user from both the app and the file system.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveUserAvatar()
        {
            var file = await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.Id.PublicKey + ".png");
            file.DeleteAsync();
            ResetUserAvatar();
            BroadCastUserAvatarOnReset();
        }

        private void BroadCastUserAvatarOnReset()
        {
            foreach (var friend in ToxModel.Instance.Friends)
            {
                FileTransferManager.Instance.SendNullAvatar(friend);
            }
        }

        /// <summary>
        ///     Resets the user's avatar to the default one.
        /// </summary>
        private void ResetUserAvatar()
        {
            UserAvatar = new BitmapImage(new Uri("ms-appx:///Assets/default-profile-picture.png"));
            IsUserAvatarSet = false;
        }

        public async void SetFriendAvatar(int friendNumber, MemoryStream avatarStream)
        {
            var file = await _avatarsFolder.CreateFileAsync(ToxModel.Instance.GetFriendPublicKey(friendNumber) + ".png",
                CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, avatarStream.ToArray());

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var friendAvatar = new BitmapImage();
                    await friendAvatar.SetSourceAsync(stream);
                    FriendAvatars[friendNumber] = friendAvatar;
                    if (FriendAvatarChanged != null)
                        FriendAvatarChanged(this, friendNumber);
                }
            });
        }

        public event EventHandler<int> FriendAvatarChanged;
        public event EventHandler UserAvatarChanged;
        public event EventHandler IsUserAvatarSetChanged;
    }
}