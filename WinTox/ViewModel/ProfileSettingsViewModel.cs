using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.UI.Core;
using SharpTox.Core;
using SharpTox.Encryption;
using WinTox.Model;

namespace WinTox.ViewModel
{
    internal class ProfileSettingsViewModel : ViewModelBase
    {
        private bool _isSwitchProfileFlyoutOpen;

        public ProfileSettingsViewModel()
        {
            App.ToxModel.PropertyChanged += ToxModelPropertyChangedHandler;
            ProfileFiles = new ObservableCollection<StorageFile>();
        }

        public ToxId Id
        {
            get { return App.ToxModel.Id; }
        }

        public string Name
        {
            get { return App.ToxModel.Name; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (value == String.Empty || lengthInBytes > ToxConstants.MaxNameLength ||
                    App.ToxModel.Name == value)
                    return;
                App.ToxModel.Name = value;
                RaisePropertyChanged();
            }
        }

        public string StatusMessage
        {
            get { return App.ToxModel.StatusMessage; }
            set
            {
                var lengthInBytes = Encoding.Unicode.GetBytes(value).Length;
                if (lengthInBytes > ToxConstants.MaxStatusMessageLength)
                    return;
                App.ToxModel.StatusMessage = value;
                RaisePropertyChanged();
            }
        }

        public ToxUserStatus Status
        {
            get { return App.ToxModel.Status; }
            set
            {
                App.ToxModel.Status = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<StorageFile> ProfileFiles { get; set; }

        public bool IsSwitchProfileFlyoutOpen
        {
            get { return _isSwitchProfileFlyoutOpen; }
            set
            {
                _isSwitchProfileFlyoutOpen = value;
                RaisePropertyChanged();
            }
        }

        private void ToxModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { RaisePropertyChanged(e.PropertyName); });
        }

        public async Task SaveDataAsync()
        {
            await App.ToxModel.SaveDataAsync();
        }

        /// <summary>
        ///     Exports the current profile to the selected file.
        /// </summary>
        /// <param name="file">The selected file.</param>
        /// <param name="password">Password (optional) to encrypt the profile with.</param>
        /// <returns>Return true on success, false otherwise.</returns>
        public async Task<bool> ExportProfile(StorageFile file, string password)
        {
            CachedFileManager.DeferUpdates(file);
            await FileIO.WriteTextAsync(file, string.Empty); // Clear the content of the file before writing to it.
            await FileIO.WriteBytesAsync(file, GetData(password));
            var status = await CachedFileManager.CompleteUpdatesAsync(file);
            return status == FileUpdateStatus.Complete;
        }

        private byte[] GetData(string password)
        {
            if (password == String.Empty)
                return App.ToxModel.GetData().Bytes;
            var encryptionKey = new ToxEncryptionKey(password);
            return App.ToxModel.GetData(encryptionKey).Bytes;
        }

        public void RandomizeNospam()
        {
            var rand = new Random();
            var nospam = new byte[4];
            rand.NextBytes(nospam);
            App.ToxModel.SetNospam(BitConverter.ToUInt32(nospam, 0));
            RaisePropertyChanged("Id");
        }

        public async Task SetCurrentProfile(StorageFile file)
        {
            var data = (await FileIO.ReadBufferAsync(file)).ToArray();
            App.ToxModel.SetCurrent(new ExtendedTox(new ToxOptions(), ToxData.FromBytes(data)));
            await App.ToxModel.SaveDataAsync();
            App.ToxModel.Start();
        }

        public async Task SwitchProfile(StorageFile file)
        {
            await SetCurrentProfile(file);
            IsSwitchProfileFlyoutOpen = false;
        }

        public async Task CreateNewProfile()
        {
            var tox = new ExtendedTox(new ToxOptions(true, true))
            {
                Name = "User",
                StatusMessage = "Using WinTox."
            };
            App.ToxModel.SetCurrent(tox);
            await App.ToxModel.SaveDataAsync();
            App.ToxModel.Start();
        }

        public async Task RefreshProfileList()
        {
            var fileList = await ApplicationData.Current.RoamingFolder.GetFilesAsync();
            ProfileFiles.Clear();
            foreach (var file in fileList)
            {
                ProfileFiles.Add(file);
            }
        }
    }
}