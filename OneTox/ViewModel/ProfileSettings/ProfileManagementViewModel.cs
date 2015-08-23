using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using OneTox.Common;
using OneTox.Helpers;
using OneTox.Model;
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
            Profiles = new ObservableCollection<ProfileViewModel>();
        }

        public ObservableCollection<ProfileViewModel> Profiles { get; set; }

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
                               var profile = ProfileViewModel.GetDefaultProfileViewModel();
                               await profile.SetAsCurrent();
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
                                       var profile = await ProfileViewModel.GetProfileViewModelFromFile(file);
                                       Profiles.Add(profile);
                                   }
                               }
                           }));
            }
        }

        public async Task SwitchProfile(ProfileViewModel profile)
        {
            await profile.SetAsCurrent();
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
                var profile = await ProfileViewModel.GetProfileViewModelFromFile(file);
                await profile.SetAsCurrent();
            }
        }

        private async Task<StorageFile> PickSourceFile()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".tox");
            return await openPicker.PickSingleFileAsync();
        }

        #endregion
    }
}