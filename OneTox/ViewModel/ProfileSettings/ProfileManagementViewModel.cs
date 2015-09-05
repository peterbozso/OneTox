using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using GalaSoft.MvvmLight.Command;
using OneTox.Config;
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
        private RelayCommand<ProfileViewModel> _deleteProfileCommand;
        private RelayCommand<string> _exportProfileCommand;
        private RelayCommand _importProfileCommand;
        private RelayCommand<ProfileViewModel> _switchProfileCommand;
        private readonly IDataService _dataService;
        private readonly IToxModel _toxModel;
        private readonly IAvatarManager _avatarManager;

        public ProfileManagementViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _toxModel = dataService.ToxModel;
            _avatarManager = dataService.AvatarManager;

            Profiles = new ObservableCollection<ProfileViewModel>();
        }

        public RelayCommand CreateNewProfileCommand
        {
            get
            {
                return _createNewProfileCommand
                       ?? (_createNewProfileCommand = new RelayCommand(
                           async () =>
                           {
                               var toxInstance = new ExtendedTox(new ToxOptions(true, true))
                               {
                                   Name = "User",
                                   StatusMessage = "Using OneTox."
                               };

                               _toxModel.SetCurrent(toxInstance);
                               await _toxModel.SaveDataAsync();
                               _toxModel.Start();
                               await _avatarManager.LoadAvatars();

                               await RefreshProfileList();
                           }));
            }
        }

        public RelayCommand<ProfileViewModel> DeleteProfileCommand
        {
            get
            {
                return _deleteProfileCommand ??
                       (_deleteProfileCommand = new RelayCommand<ProfileViewModel>(async profile =>
                       {
                           Profiles.Remove(profile);
                           await profile.DeleteBackingFile();
                       }));
            }
        }

        public ObservableCollection<ProfileViewModel> Profiles { get; }

        public RelayCommand<ProfileViewModel> SwitchProfileCommand
        {
            get
            {
                return _switchProfileCommand ??
                       (_switchProfileCommand = new RelayCommand<ProfileViewModel>(async profile =>
                       {
                           await profile.SetAsCurrent();
                           await RefreshProfileList();
                       }));
            }
        }

        public async Task RefreshProfileList()
        {
            var fileList = await ApplicationData.Current.RoamingFolder.GetFilesAsync();
            Profiles.Clear();
            foreach (var file in fileList)
            {
                if (file.FileType == ".tox")
                {
                    var profile = await ProfileViewModel.GetProfileViewModelFromFile(_dataService, file);

                    if (profile == null)
                        // It's a corrupted file: better get rid of it and don't waste time with it anymore!
                    {
                        await file.DeleteAsync();
                        continue;
                    }

                    if (profile.Id == _toxModel.Id) // Don't include the current profile in this list.
                        continue;

                    Profiles.Add(profile);
                }
            }
        }

        #region Export profile

        public RelayCommand<string> ExportProfileCommand
        {
            get
            {
                return _exportProfileCommand ?? (_exportProfileCommand = new RelayCommand<string>(async password =>
                {
                    var file = await PickDestinationFile();
                    if (file != null)
                    {
                        await FileIO.WriteBytesAsync(file, GetData(password));
                    }
                }));
            }
        }

        private byte[] GetData(string password)
        {
            if (password == string.Empty)
                return _toxModel.GetData().Bytes;
            var encryptionKey = new ToxEncryptionKey(password);
            return _toxModel.GetData(encryptionKey).Bytes;
        }

        private async Task<StorageFile> PickDestinationFile()
        {
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Tox save file", new List<string> {".tox"});
            savePicker.SuggestedFileName = _toxModel.Name;
            var file = await savePicker.PickSaveFileAsync();
            return file;
        }

        #endregion Export profile

        #region Import profile

        public RelayCommand ImportProfileCommand
        {
            get
            {
                return _importProfileCommand ?? (_importProfileCommand = new RelayCommand(async () =>
                {
                    var file = await PickSourceFile();
                    if (file != null)
                    {
                        var profile = await ProfileViewModel.GetProfileViewModelFromFile(_dataService, file);
                        if (profile == null)
                        {
                            var msgDialog = new MessageDialog(
                                "Importing profile failed because of corrupted .tox file.", "Error occurred");
                            await msgDialog.ShowAsync();
                            return;
                        }
                        await profile.SetAsCurrent();
                        await RefreshProfileList();
                    }
                }));
            }
        }

        private async Task<StorageFile> PickSourceFile()
        {
            var openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".tox");
            return await openPicker.PickSingleFileAsync();
        }

        #endregion Import profile
    }
}