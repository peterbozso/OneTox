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
using GalaSoft.MvvmLight.Command;
using OneTox.Config;
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
        private readonly IToxModel _toxModel;
        private readonly IAvatarManager _avatarManager;

        public ProfileSettingsViewModel(IDataService dataService)
        {
            _toxModel = dataService.ToxModel;
            _avatarManager = dataService.AvatarManager;

            _toxModel.PropertyChanged += ToxModelPropertyChangedHandler;
            _avatarManager.UserAvatarChanged += UserAvatarChangedHandler;
            _avatarManager.IsUserAvatarSetChanged += IsUserAvatarSetChangedHandler;
            RefreshQrCodeId();
        }

        public async Task SaveDataAsync()
        {
            await _toxModel.SaveDataAsync();
        }

        #region Avatar

        private RelayCommand _removeAvatarCommand;
        public BitmapImage Avatar => _avatarManager.UserAvatar;

        public bool IsAvatarSet => _avatarManager.IsUserAvatarSet;

        public RelayCommand RemoveAvatarCommand
        {
            get
            {
                return _removeAvatarCommand ??
                       (_removeAvatarCommand =
                           new RelayCommand(async () => { await _avatarManager.RemoveUserAvatar(); }));
            }
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

        private void IsUserAvatarSetChangedHandler(object sender, EventArgs e)
        {
            RaisePropertyChanged("IsAvatarSet");
        }

        private async Task<string> LoadUserAvatar(StorageFile file)
        {
            try
            {
                await _avatarManager.ChangeUserAvatar(file);
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

        private async Task<StorageFile> PickUserAvatar()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".png");
            return await openPicker.PickSingleFileAsync();
        }

        private void UserAvatarChangedHandler(object sender, EventArgs e)
        {
            RaisePropertyChanged("Avatar");
        }

        #endregion Avatar

        #region Tox ID

        private WriteableBitmap _qrCodeId;
        private RelayCommand _randomizeNoSpamCommand;

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

        public RelayCommand RandomizeNoSpamCommand
        {
            get
            {
                return _randomizeNoSpamCommand ?? (_randomizeNoSpamCommand = new RelayCommand(async () =>
                {
                    var rand = new Random();
                    var nospam = new byte[4];
                    rand.NextBytes(nospam);
                    _toxModel.SetNospam(BitConverter.ToUInt32(nospam, 0));
                    await _toxModel.SaveDataAsync();
                    RaisePropertyChanged("TextId");
                    RefreshQrCodeId();
                }));
            }
        }

        public ToxId TextId => _toxModel.Id;

        public void CopyToxIdToClipboard()
        {
            var dataPackage = new DataPackage {RequestedOperation = DataPackageOperation.Copy};
            dataPackage.SetText(TextId.ToString());
            Clipboard.SetContent(dataPackage);
        }

        private void RefreshQrCodeId()
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 120,
                    Width = 120,
                    Margin = 0
                }
            };

            QrCodeId = writer.Write(TextId.ToString()).ToBitmap() as WriteableBitmap;
        }

        #endregion Tox ID

        #region Other user data

        public string Name
        {
            get { return _toxModel.Name; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (_toxModel.Name == value || value == string.Empty ||
                    lengthInBytes > ToxConstants.MaxNameLength)
                    return;
                _toxModel.Name = value;
                RaisePropertyChanged();
            }
        }

        public ToxUserStatus Status
        {
            get { return _toxModel.Status; }
            set
            {
                if (value == _toxModel.Status)
                    return;
                _toxModel.Status = value;
                RaisePropertyChanged();
            }
        }

        public string StatusMessage
        {
            get { return _toxModel.StatusMessage; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (_toxModel.StatusMessage == value || value == string.Empty ||
                    lengthInBytes > ToxConstants.MaxStatusMessageLength)
                    return;
                _toxModel.StatusMessage = value;
                RaisePropertyChanged();
            }
        }

        private async void ToxModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { RaisePropertyChanged(e.PropertyName); });

            if (e.PropertyName == string.Empty)
            {
                RefreshQrCodeId();
            }
        }

        #endregion Other user data
    }
}