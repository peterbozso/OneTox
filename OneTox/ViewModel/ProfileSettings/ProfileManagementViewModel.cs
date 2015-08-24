﻿using OneTox.Common;
using OneTox.Helpers;
using OneTox.Model;
using SharpTox.Encryption;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace OneTox.ViewModel.ProfileSettings
{
    public class ProfileManagementViewModel : ObservableObject
    {
        private RelayCommand _createNewProfileCommand;

        public ProfileManagementViewModel()
        {
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
                               var profile = ProfileViewModel.GetDefaultProfileViewModel();
                               await profile.SetAsCurrent();
                               await RefreshProfileList();
                           }));
            }
        }

        public ObservableCollection<ProfileViewModel> Profiles { get; set; }

        public async Task RefreshProfileList()
        {
            var fileList = await ApplicationData.Current.RoamingFolder.GetFilesAsync();
            Profiles.Clear();
            foreach (var file in fileList)
            {
                if (file.FileType == ".tox")
                {
                    var profile = await ProfileViewModel.GetProfileViewModelFromFile(file);

                    if (profile.Id == ToxModel.Instance.Id) // Don't include the current profile in this list.
                        continue;

                    Profiles.Add(profile);
                }
            }
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

        private byte[] GetData(string password)
        {
            if (password == string.Empty)
                return ToxModel.Instance.GetData().Bytes;
            var encryptionKey = new ToxEncryptionKey(password);
            return ToxModel.Instance.GetData(encryptionKey).Bytes;
        }

        private async Task<StorageFile> PickDestinationFile()
        {
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Tox save file", new List<string> { ".tox" });
            savePicker.SuggestedFileName = ToxModel.Instance.Name;
            var file = await savePicker.PickSaveFileAsync();
            return file;
        }

        #endregion Export profile

        #region Import profile

        public async Task ImportProfile()
        {
            var file = await PickSourceFile();
            if (file != null)
            {
                var profile = await ProfileViewModel.GetProfileViewModelFromFile(file);
                await profile.SetAsCurrent();
                await RefreshProfileList();
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
