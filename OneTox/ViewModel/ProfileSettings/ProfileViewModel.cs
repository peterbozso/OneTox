using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using OneTox.Config;
using OneTox.Model;
using OneTox.Model.Avatars;
using SharpTox.Core;

namespace OneTox.ViewModel.ProfileSettings
{
    public class ProfileViewModel
    {
        private readonly ToxData _toxData;
        private readonly ToxDataInfo _toxDataInfo;
        private readonly IToxModel _toxModel;
        private readonly IAvatarManager _avatarManager;

        private ProfileViewModel(IDataService dataService, ToxData toxData, ToxDataInfo toxDataInfo)
        {
            _toxModel = dataService.ToxModel;
            _avatarManager = dataService.AvatarManager;

            _toxData = toxData;
            _toxDataInfo = toxDataInfo;
        }

        public ToxId Id => _toxDataInfo.Id;
        public string Name => _toxDataInfo.Name;
        public ToxUserStatus Status => _toxDataInfo.Status;
        public string StatusMessage => _toxDataInfo.StatusMessage;

        public static async Task<ProfileViewModel> GetProfileViewModelFromFile(IDataService dataService, StorageFile file)
        {
            var data = (await FileIO.ReadBufferAsync(file)).ToArray();
            var toxData = ToxData.FromBytes(data);

            ToxDataInfo toxDataInfo;
            toxData.TryParse(out toxDataInfo);
            if (toxDataInfo == null)
                return null;

            return new ProfileViewModel(dataService, toxData, toxDataInfo);
        }

        public async Task DeleteBackingFile()
        {
            var file = await ApplicationData.Current.RoamingFolder.GetFileAsync(_toxDataInfo.Id.PublicKey + ".tox");
            await file.DeleteAsync();
        }

        public async Task SetAsCurrent()
        {
            var toxInstance = new ExtendedTox(new ToxOptions(true, true), _toxData);
            _toxModel.SetCurrent(toxInstance);
            await _toxModel.SaveDataAsync();
            _toxModel.Start();
            await _avatarManager.LoadAvatars();
        }
    }
}