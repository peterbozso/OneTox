using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        #region Event handlers

        private async void FriendConnectionStatusChangedHandler(object sender,
            ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (ToxModel.Instance.IsFriendOnline(e.FriendNumber))
            {
                Debug.WriteLine("Friend just came online: {0}, status: {1}, name: {2}", e.FriendNumber, e.Status,
                    ToxModel.Instance.GetFriendName(e.FriendNumber));

                var file = await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.Id.PublicKey + ".png");
                if (file != null)
                {
                    await AvatarTransferManager.Instance.SendAvatar(e.FriendNumber, (StorageFile) file);
                }
                else // We have no saved avatar for the user: we have no avatar set.
                {
                    AvatarTransferManager.Instance.SendNullAvatar(e.FriendNumber);
                }
            }
            else
            {
                Debug.WriteLine("Friend just went offline: {0}", e.FriendNumber);
            }
        }

        #endregion

        #region Properties

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

        #endregion

        #region Events

        public event EventHandler<int> FriendAvatarChanged;
        public event EventHandler UserAvatarChanged;
        public event EventHandler IsUserAvatarSetChanged;

        #endregion

        #region Friend avatar management

        public async Task<StorageFile> GetFriendAvatarFile(int friendNumber)
        {
            return await _avatarsFolder.GetFileAsync(ToxModel.Instance.GetFriendPublicKey(friendNumber) + ".png");
        }

        public async Task RemoveFriendAvatar(int friendNumber)
        {
            FriendAvatars.Remove(friendNumber);
            if (FriendAvatarChanged != null)
                FriendAvatarChanged(this, friendNumber);
            await DeleteFriendAvatarFile(friendNumber);
        }

        private async Task DeleteFriendAvatarFile(int friendNumber)
        {
            var file = await GetFriendAvatarFile(friendNumber);
            await file.DeleteAsync();
        }

        public async void ChangeFriendAvatar(int friendNumber, MemoryStream avatarStream)
        {
            var file = await SaveFriendAvatar(friendNumber, avatarStream);
            await SetFriendAvatar(friendNumber, file);
        }

        /// <summary>
        ///     Saves the friend's avatar.
        /// </summary>
        /// <param name="friendNumber">Friend number of the friend we are saving the avatar for.</param>
        /// <param name="avatarStream">The stream that contains the avatar's data.</param>
        /// <returns>The file where the avatar was saved.</returns>
        private async Task<StorageFile> SaveFriendAvatar(int friendNumber, MemoryStream avatarStream)
        {
            var file = await _avatarsFolder.CreateFileAsync(ToxModel.Instance.GetFriendPublicKey(friendNumber) + ".png",
                CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, avatarStream.ToArray());
            return file;
        }

        private async Task SetFriendAvatar(int friendNumber, StorageFile file)
        {
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

        #endregion

        #region Loading avatars

        // We presume that this is called before any other function that use _avatarsFolder.
        public async Task LoadAvatars()
        {
            _avatarsFolder =
                await ApplicationData.Current.RoamingFolder.CreateFolderAsync(
                    "avatars", CreationCollisionOption.OpenIfExists);

            await LoadUserAvatar();
            await LoadFriendsAvatars();
        }

        private async Task LoadUserAvatar()
        {
            var file = await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.Id.PublicKey + ".png") as StorageFile;
            if (file != null)
                await SetUserAvatar(file);
        }

        private async Task LoadFriendsAvatars()
        {
            foreach (var friendNumber in ToxModel.Instance.Friends)
            {
                var publicKey = ToxModel.Instance.GetFriendPublicKey(friendNumber);
                var file = await _avatarsFolder.TryGetItemAsync(publicKey + ".png") as StorageFile;
                if (file == null)
                    continue;
                await SetFriendAvatar(friendNumber, file);
            }
        }

        #endregion

        #region User avatar management

        public async Task ChangeUserAvatar(StorageFile file)
        {
            await SetUserAvatar(file);
            var copy = await CopyUserAvatarFile(file);
            await BroadcastUserAvatarOnSet(copy);
        }

        private async Task SetUserAvatar(StorageFile file)
        {
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                // TODO: Allow bigger avatars and implement resizing.
                if (stream.AsStream().Length > KMaxPictureSize)
                    throw new ArgumentOutOfRangeException();
                UserAvatar.SetSource(stream);
                IsUserAvatarSet = true;
            }
        }

        /// <summary>
        ///     Copies the user's avatar's file to it's place (avatars subfolder).
        /// </summary>
        /// <param name="file">The user's avatar's file.</param>
        /// <returns>The copy of the file.</returns>
        private async Task<StorageFile> CopyUserAvatarFile(StorageFile file)
        {
            var copy = await file.CopyAsync(_avatarsFolder);
            await copy.RenameAsync(ToxModel.Instance.Id.PublicKey + ".png", NameCollisionOption.ReplaceExisting);
            return copy;
        }

        private async Task BroadcastUserAvatarOnSet(StorageFile file)
        {
            foreach (var friend in ToxModel.Instance.Friends)
            {
                await AvatarTransferManager.Instance.SendAvatar(friend, file);
            }
        }

        /// <summary>
        ///     Removes the current avatar of the user from both the app and the file system.
        /// </summary>
        /// <returns></returns>
        public async Task RemoveUserAvatar()
        {
            ResetUserAvatar();
            await DeleteUserAvatarFile();
            BroadCastUserAvatarOnReset();
        }

        /// <summary>
        ///     Resets the user's avatar to the default one.
        /// </summary>
        private void ResetUserAvatar()
        {
            UserAvatar = new BitmapImage(new Uri("ms-appx:///Assets/default-profile-picture.png"));
            IsUserAvatarSet = false;
        }

        private async Task DeleteUserAvatarFile()
        {
            var file = await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.Id.PublicKey + ".png");
            file.DeleteAsync();
        }

        private void BroadCastUserAvatarOnReset()
        {
            foreach (var friend in ToxModel.Instance.Friends)
            {
                AvatarTransferManager.Instance.SendNullAvatar(friend);
            }
        }

        #endregion
    }
}