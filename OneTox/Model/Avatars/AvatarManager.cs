using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using SharpTox.Core;

namespace OneTox.Model.Avatars
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class AvatarManager
    {
        // See: https://github.com/irungentoo/Tox_Client_Guidelines/blob/master/Important/Avatars.md
        private const int KMaxPictureSizeInBytes = 1 << 16;
        private static AvatarManager _instance;
        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        private StorageFolder _avatarsFolder;
        private bool _isUserAvatarSet;
        private BitmapImage _userAvatar;

        private AvatarManager()
        {
            ResetUserAvatar();
            ToxModel.Instance.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
            FriendAvatars = new Dictionary<int, BitmapImage>();
        }

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
                IsUserAvatarSetChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public BitmapImage UserAvatar
        {
            get { return _userAvatar; }
            private set
            {
                _userAvatar = value;
                UserAvatarChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Dictionary<int, BitmapImage> FriendAvatars { get; }

        #endregion

        #region Events

        public event EventHandler<int> FriendAvatarChanged;
        public event EventHandler UserAvatarChanged;
        public event EventHandler IsUserAvatarSetChanged;

        #endregion

        #region Friend avatar management

        public async Task<Stream> GetFriendAvatarStream(int friendNumber)
        {
            var friendAvatarFile = await GetFriendAvatarFile(friendNumber);
            if (friendAvatarFile == null)
                return null;
            return (await friendAvatarFile.OpenReadAsync()).AsStreamForRead();
        }

        public async Task RemoveFriendAvatar(int friendNumber)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                FriendAvatars.Remove(friendNumber);
                RaiseFriendAvatarChanged(friendNumber);
                await DeleteFriendAvatarFile(friendNumber);
            });
        }

        private async Task DeleteFriendAvatarFile(int friendNumber)
        {
            var friendAvatarFile = await GetFriendAvatarFile(friendNumber);
            if (friendAvatarFile != null)
                await friendAvatarFile.DeleteAsync();
        }

        private async Task<StorageFile> GetFriendAvatarFile(int friendNumber)
        {
            return
                await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.GetFriendPublicKey(friendNumber) + ".png") as
                    StorageFile;
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
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var friendAvatar = new BitmapImage();
                    var successFulSetSource = await TrySetAvatarSource(friendAvatar, stream, friendNumber);
                    if (!successFulSetSource)
                    {
                        await DeleteFriendAvatarFile(friendNumber);
                        return;
                    }
                    FriendAvatars[friendNumber] = friendAvatar;
                    RaiseFriendAvatarChanged(friendNumber);
                }
            });
        }

        private async Task<bool> TrySetAvatarSource(BitmapImage bitmap, IRandomAccessStream stream, int friendNumber)
        {
            try
            {
                await bitmap.SetSourceAsync(stream);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void RaiseFriendAvatarChanged(int friendNumber)
        {
            FriendAvatarChanged?.Invoke(this, friendNumber);
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
            StorageFile newFile;
            if (await AvatarResizer.IsAvatarTooBig(file))
            {
                var avatarResizer = new AvatarResizer(_avatarsFolder, file);
                newFile = await avatarResizer.SaveUserAvatarFile();
            }
            else
            {
                newFile = await SaveUserAvatarFile(file);
            }
            await SetUserAvatar(newFile);
            await BroadcastUserAvatarOnSet(newFile);
        }

        private async Task SetUserAvatar(StorageFile file)
        {
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                if (stream.AsStream().Length > KMaxPictureSizeInBytes) // TODO: Remove this safety check later!
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
        private async Task<StorageFile> SaveUserAvatarFile(StorageFile file)
        {
            var copy = await file.CopyAsync(_avatarsFolder);
            await copy.RenameAsync(ToxModel.Instance.Id.PublicKey + ".png", NameCollisionOption.ReplaceExisting);
            return copy;
        }

        private async Task BroadcastUserAvatarOnSet(StorageFile file)
        {
            foreach (var friendNumber in ToxModel.Instance.Friends)
            {
                if (ToxModel.Instance.IsFriendOnline(friendNumber))
                    await SendUserAvatar(friendNumber, file);
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
            await file.DeleteAsync();
        }

        private void BroadCastUserAvatarOnReset()
        {
            foreach (var friendNumber in ToxModel.Instance.Friends)
            {
                if (ToxModel.Instance.IsFriendOnline(friendNumber))
                    AvatarTransferManager.Instance.SendNullAvatar(friendNumber);
            }
        }

        private async void FriendConnectionStatusChangedHandler(object sender,
            ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (ToxModel.Instance.IsFriendOnline(e.FriendNumber))
            {
                Debug.WriteLine("Friend just came online: {0}, status: {1}, name: {2}", e.FriendNumber, e.Status,
                    ToxModel.Instance.GetFriendName(e.FriendNumber));

                var file = await _avatarsFolder.TryGetItemAsync(ToxModel.Instance.Id.PublicKey + ".png") as StorageFile;
                if (file != null)
                {
                    await SendUserAvatar(e.FriendNumber, file);
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

        private async Task SendUserAvatar(int friendNumber, StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            AvatarTransferManager.Instance.SendAvatar(friendNumber, stream, file.Name);
        }

        private class AvatarResizer
        {
            private readonly StorageFile _avatarFile;
            private readonly StorageFolder _avatarsFolder;

            public AvatarResizer(StorageFolder avatarsFolder, StorageFile avatarFile)
            {
                _avatarsFolder = avatarsFolder;
                _avatarFile = avatarFile;
            }

            public async Task<StorageFile> SaveUserAvatarFile()
            {
                var avatar = await LoadAvatarToWriteableBitmap();
                avatar = ResizeAvatar(avatar);
                return await SaveUserAvatar(avatar);
            }

            public static async Task<bool> IsAvatarTooBig(StorageFile file)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    return (stream.AsStream().Length > KMaxPictureSizeInBytes);
                }
            }

            private async Task<WriteableBitmap> LoadAvatarToWriteableBitmap()
            {
                using (var stream = await _avatarFile.OpenAsync(FileAccessMode.Read))
                {
                    var bmpImg = new BitmapImage();
                    bmpImg.SetSource(stream);

                    var writeableBitmap = new WriteableBitmap(bmpImg.PixelWidth, bmpImg.PixelHeight);
                    stream.Seek(0);
                    writeableBitmap.SetSource(stream);

                    return writeableBitmap;
                }
            }

            private WriteableBitmap ResizeAvatar(WriteableBitmap writeableBitmap)
            {
                var resized = CropIfNeeded(writeableBitmap);
                var size = Convert.ToInt32((double) Application.Current.Resources["DefaultAvatarSize"]);
                return resized.Resize(size, size, WriteableBitmapExtensions.Interpolation.Bilinear);
            }

            private WriteableBitmap CropIfNeeded(WriteableBitmap writeableBitmap)
            {
                var height = writeableBitmap.PixelHeight;
                var width = writeableBitmap.PixelWidth;
                var xCenter = width/2;
                var yCenter = height/2;

                if (width > height)
                {
                    return writeableBitmap.Crop(xCenter - height/2, 0, height, height);
                }

                if (width < height)
                {
                    return writeableBitmap.Crop(0, yCenter - width/2, width, width);
                }

                return writeableBitmap;
            }

            // Kudos: http://stackoverflow.com/questions/17140774/how-to-save-a-writeablebitmap-as-a-file
            private async Task<StorageFile> SaveUserAvatar(WriteableBitmap avatar)
            {
                var file = await _avatarsFolder.CreateFileAsync(ToxModel.Instance.Id.PublicKey + ".png",
                    CreationCollisionOption.ReplaceExisting);
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    var pixelStream = avatar.PixelBuffer.AsStream();
                    var pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                        (uint) avatar.PixelWidth,
                        (uint) avatar.PixelHeight,
                        96.0,
                        96.0,
                        pixels);
                    await encoder.FlushAsync();
                }
                return file;
            }
        }

        #endregion
    }
}