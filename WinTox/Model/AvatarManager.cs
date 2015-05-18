using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace WinTox.Model
{
    public class AvatarManager
    {
        private const int KMaxPictureSize = 1 << 16;
        // See: https://github.com/irungentoo/Tox_Client_Guidelines/blob/master/Important/Avatars.md

        private BitmapImage _userAvatar;

        public AvatarManager()
        {
            UserAvatar = new BitmapImage(new Uri("ms-appx:///Assets/default-profile-picture.png"));
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

        public async Task LoadUserAvatar(StorageFile file)
        {
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                if (stream.AsStream().Length > KMaxPictureSize)
                    throw new ArgumentOutOfRangeException();
                UserAvatar.SetSource(stream);
            }
        }
    }
}