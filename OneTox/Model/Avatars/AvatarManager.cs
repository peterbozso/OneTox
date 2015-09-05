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
    public class AvatarManager : IAvatarManager
    {
        // See: https://github.com/irungentoo/Tox_Client_Guidelines/blob/master/Important/Avatars.md
        private const int KMaxPictureSizeInBytes = 1 << 16;
        private readonly AvatarTransferManager _avatarTransferManager;

        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        private readonly IToxModel _toxModel;
        private StorageFolder _avatarsFolder;
        private bool _isUserAvatarSet;
        private BitmapImage _userAvatar;

        public AvatarManager(IToxModel toxModel)
        {
            _toxModel = toxModel;
            _avatarTransferManager = new AvatarTransferManager(toxModel, this);

            ResetUserAvatar();
            FriendAvatars = new Dictionary<int, BitmapImage>();
            _toxModel.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
        }

        #region Properties

        public Dictionary<int, BitmapImage> FriendAvatars { get; }

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

        #endregion Properties

        #region Events

        public event EventHandler<int> FriendAvatarChanged;

        public event EventHandler IsUserAvatarSetChanged;

        public event EventHandler UserAvatarChanged;

        #endregion Events

        #region Friend avatar management

        public async void ChangeFriendAvatar(int friendNumber, MemoryStream avatarStream)
        {
            var file = await SaveFriendAvatar(friendNumber, avatarStream);
            await SetFriendAvatar(friendNumber, file);
        }

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
                await _avatarsFolder.TryGetItemAsync(_toxModel.GetFriendPublicKey(friendNumber) + ".png") as
                    StorageFile;
        }

        private void RaiseFriendAvatarChanged(int friendNumber)
        {
            FriendAvatarChanged?.Invoke(this, friendNumber);
        }

        /// <summary>
        ///     Saves the friend's avatar.
        /// </summary>
        /// <param name="friendNumber">Friend number of the friend we are saving the avatar for.</param>
        /// <param name="avatarStream">The stream that contains the avatar's data.</param>
        /// <returns>The file where the avatar was saved.</returns>
        private async Task<StorageFile> SaveFriendAvatar(int friendNumber, MemoryStream avatarStream)
        {
            var file = await _avatarsFolder.CreateFileAsync(_toxModel.GetFriendPublicKey(friendNumber) + ".png",
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

        #endregion Friend avatar management

        #region Loading avatars

        // We presume that this is called before any other function that use _avatarsFolder.
        public async Task LoadAvatars()
        {
            _avatarsFolder =
                await ApplicationData.Current.RoamingFolder.CreateFolderAsync(
                    "avatars", CreationCollisionOption.OpenIfExists);

            await LoadUserAvatar();
            await LoadFriendAvatars();
        }

        private void ClearFriendAvatars()
        {
            FriendAvatars.Clear();

            foreach (var friendNumber in _toxModel.Friends)
            {
                RaiseFriendAvatarChanged(friendNumber);
            }
        }

        private async Task LoadFriendAvatars()
        {
            ClearFriendAvatars();

            foreach (var friendNumber in _toxModel.Friends)
            {
                var publicKey = _toxModel.GetFriendPublicKey(friendNumber);

                var file = await _avatarsFolder.TryGetItemAsync(publicKey + ".png") as StorageFile;
                if (file == null)
                    continue;

                await SetFriendAvatar(friendNumber, file);
            }
        }

        private async Task LoadUserAvatar()
        {
            var file = await _avatarsFolder.TryGetItemAsync(_toxModel.Id.PublicKey + ".png") as StorageFile;
            if (file == null)
            {
                ResetUserAvatar();
            }
            else
            {
                await SetUserAvatar(file);
            }
        }

        #endregion Loading avatars

        #region User avatar management

        public async Task ChangeUserAvatar(StorageFile file)
        {
            StorageFile newFile;
            if (await AvatarResizer.IsAvatarTooBig(file))
            {
                var avatarResizer = new AvatarResizer(_toxModel, _avatarsFolder, file);
                newFile = await avatarResizer.SaveUserAvatarFile();
            }
            else
            {
                newFile = await SaveUserAvatarFile(file);
            }
            await SetUserAvatar(newFile);
            await BroadcastUserAvatarOnSet(newFile);
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

        private void BroadCastUserAvatarOnReset()
        {
            foreach (var friendNumber in _toxModel.Friends)
            {
                if (_toxModel.IsFriendOnline(friendNumber))
                    _avatarTransferManager.SendNullAvatar(friendNumber);
            }
        }

        private async Task BroadcastUserAvatarOnSet(StorageFile file)
        {
            foreach (var friendNumber in _toxModel.Friends)
            {
                if (_toxModel.IsFriendOnline(friendNumber))
                    await SendUserAvatar(friendNumber, file);
            }
        }

        private async Task DeleteUserAvatarFile()
        {
            var file = await _avatarsFolder.TryGetItemAsync(_toxModel.Id.PublicKey + ".png");
            await file.DeleteAsync();
        }

        private async void FriendConnectionStatusChangedHandler(object sender,
            ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            if (_toxModel.IsFriendOnline(e.FriendNumber))
            {
                Debug.WriteLine("Friend just came online: {0}, status: {1}, name: {2}", e.FriendNumber, e.Status,
                    _toxModel.GetFriendName(e.FriendNumber));

                var file = await _avatarsFolder.TryGetItemAsync(_toxModel.Id.PublicKey + ".png") as StorageFile;
                if (file != null)
                {
                    await SendUserAvatar(e.FriendNumber, file);
                }
                else // We have no saved avatar for the user: we have no avatar set.
                {
                    _avatarTransferManager.SendNullAvatar(e.FriendNumber);
                }
            }
            else
            {
                Debug.WriteLine("Friend just went offline: {0}", e.FriendNumber);
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

        /// <summary>
        ///     Copies the user's avatar's file to it's place (avatars subfolder).
        /// </summary>
        /// <param name="file">The user's avatar's file.</param>
        /// <returns>The copy of the file.</returns>
        private async Task<StorageFile> SaveUserAvatarFile(StorageFile file)
        {
            var copy = await file.CopyAsync(_avatarsFolder);
            await copy.RenameAsync(_toxModel.Id.PublicKey + ".png", NameCollisionOption.ReplaceExisting);
            return copy;
        }

        private async Task SendUserAvatar(int friendNumber, StorageFile file)
        {
            var stream = (await file.OpenReadAsync()).AsStreamForRead();
            _avatarTransferManager.SendAvatar(friendNumber, stream, file.Name);
        }

        private async Task SetUserAvatar(StorageFile file)
        {
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                if (stream.AsStream().Length > KMaxPictureSizeInBytes) // TODO: Remove this safety check later!
                    throw new ArgumentOutOfRangeException();
                var newAvatar = new BitmapImage();
                newAvatar.SetSource(stream);
                UserAvatar = newAvatar;
                IsUserAvatarSet = true;
            }
        }

        private class AvatarResizer
        {
            private readonly StorageFile _avatarFile;
            private readonly StorageFolder _avatarsFolder;
            private readonly IToxModel _toxModel;

            public AvatarResizer(IToxModel toxModel, StorageFolder avatarsFolder, StorageFile avatarFile)
            {
                _toxModel = toxModel;
                _avatarsFolder = avatarsFolder;
                _avatarFile = avatarFile;
            }

            public static async Task<bool> IsAvatarTooBig(StorageFile file)
            {
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    return (stream.AsStream().Length > KMaxPictureSizeInBytes);
                }
            }

            public async Task<StorageFile> SaveUserAvatarFile()
            {
                var avatar = await LoadAvatarToWriteableBitmap();
                avatar = ResizeAvatar(avatar);
                return await SaveUserAvatar(avatar);
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

            // Kudos: http://stackoverflow.com/questions/17140774/how-to-save-a-writeablebitmap-as-a-file
            private async Task<StorageFile> SaveUserAvatar(WriteableBitmap avatar)
            {
                var file = await _avatarsFolder.CreateFileAsync(_toxModel.Id.PublicKey + ".png",
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

        #endregion User avatar management
    }
}