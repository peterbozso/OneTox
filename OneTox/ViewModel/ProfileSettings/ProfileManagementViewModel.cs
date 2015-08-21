using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using OneTox.Common;
using OneTox.Helpers;
using OneTox.Model;
using OneTox.Model.Avatars;
using SharpTox.Core;
using SharpTox.Encryption;

namespace OneTox.ViewModel.ProfileSettings
{
    public class ProfileManagementViewModel : ObservableObject
    {
        private RelayCommand _createNewProfileCommand;
        private bool _isSwitchProfileFlyoutClosed;
        private RelayCommand _refreshProfileListCommand;

        public ProfileManagementViewModel()
        {
            Profiles = new ObservableCollection<ExtendedTox>();
        }

        public ObservableCollection<ExtendedTox> Profiles { get; set; }

        public bool IsSwitchProfileFlyoutClosed
        {
            get { return _isSwitchProfileFlyoutClosed; }
            set
            {
                if (value == _isSwitchProfileFlyoutClosed)
                    return;
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
                                   StatusMessage = "Using OneTox."
                               };
                               await SetCurrentProfile(tox);
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
                               Profiles.Clear();
                               foreach (var file in fileList)
                               {
                                   if (file.FileType == ".tox")
                                   {
                                       var tox = await LoadToxInstanceFromFile(file);
                                       Profiles.Add(tox);
                                   }
                               }
                           }));
            }
        }

        public async Task SwitchProfile(ExtendedTox tox)
        {
            await SetCurrentProfile(tox);
            IsSwitchProfileFlyoutClosed = true;
        }

        private async Task<ExtendedTox> LoadToxInstanceFromFile(StorageFile file)
        {
            var data = (await FileIO.ReadBufferAsync(file)).ToArray();
            return new ExtendedTox(new ToxOptions(true, true), ToxData.FromBytes(data));
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
            savePicker.SuggestedFileName = ToxModel.Instance.Name;
            var file = await savePicker.PickSaveFileAsync();
            return file;
        }

        private byte[] GetData(string password)
        {
            if (password == string.Empty)
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
                var tox = await LoadToxInstanceFromFile(file);
                await SetCurrentProfile(tox);
            }
        }

        private async Task<StorageFile> PickSourceFile()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".tox");
            return await openPicker.PickSingleFileAsync();
        }

        public async Task SetCurrentProfile(ExtendedTox tox)
        {
            ToxModel.Instance.SetCurrent(tox);
            await ToxModel.Instance.SaveDataAsync();
            ToxModel.Instance.Start();
            await AvatarManager.Instance.LoadAvatars();
        }

        #endregion
    }
}