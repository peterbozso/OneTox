using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
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
        private BitmapImage _userAvatar;

        private AvatarManager()
        {
            UserAvatar = new BitmapImage(new Uri("ms-appx:///Assets/default-profile-picture.png"));
            ToxModel.Instance.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
        }

        public static AvatarManager Instance
        {
            get { return _instance ?? (_instance = new AvatarManager()); }
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

        public event EventHandler UserAvatarChanged;

        public async Task ChangeUserAvatar(StorageFile file)
        {
            await SetUserAvatar(file);
            var copy = await file.CopyAsync(_avatarsFolder);
            await copy.RenameAsync(ToxModel.Instance.Id.PublicKey + ".png", NameCollisionOption.ReplaceExisting);
            await BroadcastUserAvatar(copy);
        }

        // We presume that this is called before any other function that use _avatarsFolder.
        public async Task LoadUserAvatar()
        {
            _avatarsFolder =
                await ApplicationData.Current.RoamingFolder.CreateFolderAsync(
                    "avatars", CreationCollisionOption.OpenIfExists);

            var file = await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.Id.PublicKey + ".png");
            if (file != null)
                await SetUserAvatar(file as StorageFile);
        }

        private async Task SetUserAvatar(StorageFile file)
        {
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                if (stream.AsStream().Length > KMaxPictureSize)
                    throw new ArgumentOutOfRangeException();
                UserAvatar.SetSource(stream);
            }
        }

        private async Task BroadcastUserAvatar(StorageFile file)
        {
            foreach (var friend in ToxModel.Instance.Friends)
            {
                await FileTransferManager.Instance.SendFile(friend, ToxFileKind.Avatar, file);
            }
        }

        private async void FriendConnectionStatusChangedHandler(object sender,
            ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (e.Status != ToxConnectionStatus.None)
            {
                var file = await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.Id.PublicKey + ".png");
                if (file != null)
                {
                    await FileTransferManager.Instance.SendFile(e.FriendNumber, ToxFileKind.Avatar, (StorageFile) file);
                }
                else // We have no saved avatar for the user: we have no avatar set.
                {
                    FileTransferManager.Instance.SendNullAvatar(e.FriendNumber);
                }
            }
        }
    }
}