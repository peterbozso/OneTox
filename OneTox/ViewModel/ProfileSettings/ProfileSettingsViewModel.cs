using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using OneTox.Common;
using OneTox.Helpers;
using OneTox.Model;
using OneTox.Model.Avatars;
using SharpTox.Core;
using ZXing;
using ZXing.Common;

namespace OneTox.ViewModel.ProfileSettings
{
    internal class ProfileSettingsViewModel : ObservableObject
    {
        public ProfileSettingsViewModel()
        {
            ToxModel.Instance.PropertyChanged += ToxModelPropertyChangedHandler;
            AvatarManager.Instance.UserAvatarChanged += UserAvatarChangedHandler;
            AvatarManager.Instance.IsUserAvatarSetChanged += IsUserAvatarSetChangedHandler;
            RefreshQrCodeId();
        }

        public async Task SaveDataAsync()
        {
            await ToxModel.Instance.SaveDataAsync();
        }

        #region Avatar

        public BitmapImage Avatar => AvatarManager.Instance.UserAvatar;

        public bool IsAvatarSet => AvatarManager.Instance.IsUserAvatarSet;

        private void IsUserAvatarSetChangedHandler(object sender, EventArgs e)
        {
            RaisePropertyChanged("IsAvatarSet");
        }

        private void UserAvatarChangedHandler(object sender, EventArgs e)
        {
            RaisePropertyChanged("Avatar");
        }

        public async Task ChangeAvatar()
        {
            var newAvatarFile = await PickUserAvatar();
            if (newAvatarFile != null)
            {
                var errorMessage = await LoadUserAvatar(newAvatarFile);
                if (errorMessage != string.Empty)
                {
                    var msgDialog = new MessageDialog(errorMessage, "Unsuccessful loading");
                    await msgDialog.ShowAsync();
                }
            }
        }

        private async Task<StorageFile> PickUserAvatar()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".png");
            return await openPicker.PickSingleFileAsync();
        }

        private async Task<string> LoadUserAvatar(StorageFile file)
        {
            try
            {
                await AvatarManager.Instance.ChangeUserAvatar(file);
            }
            catch (ArgumentOutOfRangeException)
            {
                return "The picture is too big!";
            }
            catch
            {
                return "The picture is corrupted!";
            }
            return string.Empty;
        }

        private RelayCommand _removeAvatarCommand;

        public RelayCommand RemoveAvatarCommand
        {
            get
            {
                return _removeAvatarCommand ??
                       (_removeAvatarCommand =
                           new RelayCommand(async () => { await AvatarManager.Instance.RemoveUserAvatar(); }));
            }
        }

        #endregion

        #region Tox ID

        public ToxId TextId => ToxModel.Instance.Id;

        private void RefreshQrCodeId()
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 200,
                    Width = 200
                }
            };

            QrCodeId = writer.Write(TextId.ToString()).ToBitmap() as WriteableBitmap;
        }

        private WriteableBitmap _qrCodeId;

        public WriteableBitmap QrCodeId
        {
            get { return _qrCodeId; }
            set
            {
                if (value == _qrCodeId)
                    return;
                _qrCodeId = value;
                RaisePropertyChanged();
            }
        }

        public void CopyToxIdToClipboard()
        {
            var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
            dataPackage.SetText(TextId.ToString());
            Clipboard.SetContent(dataPackage);
        }

        private RelayCommand _randomizeNoSpamCommand;

        public RelayCommand RandomizeNoSpamCommand
        {
            get
            {
                return _randomizeNoSpamCommand ?? (_randomizeNoSpamCommand = new RelayCommand(async () =>
                {
                    var rand = new Random();
                    var nospam = new byte[4];
                    rand.NextBytes(nospam);
                    ToxModel.Instance.SetNospam(BitConverter.ToUInt32(nospam, 0));
                    await ToxModel.Instance.SaveDataAsync();
                    RaisePropertyChanged("TextId");
                    RefreshQrCodeId();
                }));
            }
        }

        #endregion

        #region Other user data

        public string Name
        {
            get { return ToxModel.Instance.Name; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (ToxModel.Instance.Name == value || value == string.Empty ||
                    lengthInBytes > ToxConstants.MaxNameLength)
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
                if (ToxModel.Instance.StatusMessage == value || value == string.Empty ||
                    lengthInBytes > ToxConstants.MaxStatusMessageLength)
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
                if (value == ToxModel.Instance.Status)
                    return;
                ToxModel.Instance.Status = value;
                RaisePropertyChanged();
            }
        }

        private async void ToxModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { RaisePropertyChanged(e.PropertyName); });
        }

        #endregion
    }
}