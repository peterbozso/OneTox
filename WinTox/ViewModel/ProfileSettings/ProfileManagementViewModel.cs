using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using SharpTox.Core;
using SharpTox.Encryption;
using WinTox.Common;
using WinTox.Model;

namespace WinTox.ViewModel.ProfileSettings
{
    internal class ProfileManagementViewModel : ViewModelBase
    {
        private RelayCommand _createNewProfileCommand;
        private bool _isSwitchProfileFlyoutClosed;
        private RelayCommand _refreshProfileListCommand;

        public ProfileManagementViewModel()
        {
            ProfileFiles = new ObservableCollection<StorageFile>();
        }

        public string Name
        {
            get { return ToxModel.Instance.Name; }
        }

        public ObservableCollection<StorageFile> ProfileFiles { get; set; }

        public bool IsSwitchProfileFlyoutClosed
        {
            get { return _isSwitchProfileFlyoutClosed; }
            set
            {
                _isSwitchProfileFlyoutClosed = value;
                RaisePropertyChanged();
                if (value)
                    IsSwitchProfileFlyoutClosed = false;
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
                               ToxModel.Instance.SetCurrent(tox);
                               await ToxModel.Instance.SaveDataAsync();
                               ToxModel.Instance.Start();
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

        public async Task SwitchProfile(StorageFile file)
        {
            await SetCurrentProfile(file);
            IsSwitchProfileFlyoutClosed = true;
        }

        #region Export profile

        /// <summary>
        ///     Exports the current profile to the selected file.
        /// </summary>
        /// <param name="password">Password (optional) to encrypt the profile with.</param>
        /// <returns></returns>
        public async Task ExportProfile(string password)
        {
            var file = await PickDestinationFile();
            if (file != null)
            {
                await FileIO.WriteBytesAsync(file, GetData(password));
            }
        }

        private async Task<StorageFile> PickDestinationFile()
        {
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Tox save file", new List<string> {".tox"});
            savePicker.SuggestedFileName = Name;
            var file = await savePicker.PickSaveFileAsync();
            return file;
        }

        private byte[] GetData(string password)
        {
            if (password == String.Empty)
                return ToxModel.Instance.GetData().Bytes;
            var encryptionKey = new ToxEncryptionKey(password);
            return ToxModel.Instance.GetData(encryptionKey).Bytes;
        }

        #endregion

        #region Import profile

        public async Task ImportProfile()
        {
            var file = await PickSourceFile();
            if (file != null)
            {
                await SetCurrentProfile(file);
            }
        }

        private async Task<StorageFile> PickSourceFile()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".tox");
            return await openPicker.PickSingleFileAsync();
        }

        public async Task SetCurrentProfile(StorageFile file)
        {
            var data = (await FileIO.ReadBufferAsync(file)).ToArray();
            ToxModel.Instance.SetCurrent(new ExtendedTox(new ToxOptions(true, true), ToxData.FromBytes(data)));
            await ToxModel.Instance.SaveDataAsync();
            ToxModel.Instance.Start();
        }

        #endregion
    }
}