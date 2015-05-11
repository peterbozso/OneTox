using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Provider;
using SharpTox.Core;
using SharpTox.Encryption;
using WinTox.Common;
using WinTox.Model;

namespace WinTox.ViewModel
{
    internal class ProfileManagementViewModel : ViewModelBase
    {
        private RelayCommand _createNewProfileCommand;
        private bool _isSwitchProfileFlyoutOpen;
        private RelayCommand _refreshProfileListCommand;

        public ProfileManagementViewModel()
        {
            ProfileFiles = new ObservableCollection<StorageFile>();
        }

        public string Name
        {
            get { return App.ToxModel.Name; }
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

        public RelayCommand CreateNewProfileCommand
        {
            get
            {
                return _createNewProfileCommand
                       ?? (_createNewProfileCommand = new RelayCommand(
                           async () =>
                           {
                               var tox = new ExtendedTox(new ToxOptions(true, true))
                               {
                                   Name = "User",
                                   StatusMessage = "Using WinTox."
                               };
                               App.ToxModel.SetCurrent(tox);
                               await App.ToxModel.SaveDataAsync();
                               App.ToxModel.Start();
                           }));
            }
        }

        public RelayCommand RefreshProfileListCommand
        {
            get
            {
                return _refreshProfileListCommand
                       ?? (_refreshProfileListCommand = new RelayCommand(
                           async () =>
                           {
                               var fileList = await ApplicationData.Current.RoamingFolder.GetFilesAsync();
                               ProfileFiles.Clear();
                               foreach (var file in fileList)
                               {
                                   ProfileFiles.Add(file);
                               }
                           }));
            }
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
    }
}