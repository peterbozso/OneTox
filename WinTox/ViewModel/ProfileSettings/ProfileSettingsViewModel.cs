using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel.ProfileSettings
{
    internal class ProfileSettingsViewModel : ViewModelBase
    {
        public ProfileSettingsViewModel()
        {
            ToxModel.Instance.PropertyChanged += ToxModelPropertyChangedHandler;
            AvatarManager.Instance.UserAvatarChanged += UserAvatarChangedHandler;
        }

        public ToxId Id
        {
            get { return ToxModel.Instance.Id; }
        }

        public string Name
        {
            get { return ToxModel.Instance.Name; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (value == String.Empty || lengthInBytes > ToxConstants.MaxNameLength ||
                    ToxModel.Instance.Name == value)
                    return;
                ToxModel.Instance.Name = value;
                RaisePropertyChanged();
            }
        }

        public string StatusMessage
        {
            get { return ToxModel.Instance.StatusMessage; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (lengthInBytes > ToxConstants.MaxStatusMessageLength)
                    return;
                ToxModel.Instance.StatusMessage = value;
                RaisePropertyChanged();
            }
        }

        public ToxUserStatus Status
        {
            get { return ToxModel.Instance.Status; }
            set
            {
                ToxModel.Instance.Status = value;
                RaisePropertyChanged();
            }
        }

        public BitmapImage Avatar
        {
            get { return AvatarManager.Instance.UserAvatar; }
        }

        public async Task<string> LoadUserAvatar(StorageFile file)
        {
            try
            {
                await AvatarManager.Instance.LoadUserAvatar(file);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "The picture is too big!";
            }
            catch
            {
                return "The picture is corrupted!";
            }
            return String.Empty;
        }

        private void UserAvatarChangedHandler(object sender, EventArgs e)
        {
            RaisePropertyChanged("Avatar");
        }

        private void ToxModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { RaisePropertyChanged(e.PropertyName); });
        }

        public async Task SaveDataAsync()
        {
            await ToxModel.Instance.SaveDataAsync();
        }

        public void RandomizeNospam()
        {
            var rand = new Random();
            var nospam = new byte[4];
            rand.NextBytes(nospam);
            ToxModel.Instance.SetNospam(BitConverter.ToUInt32(nospam, 0));
            RaisePropertyChanged("Id");
        }
    }
}