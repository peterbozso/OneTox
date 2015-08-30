using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using OneTox.Model;
using OneTox.Model.Avatars;
using SharpTox.Core;

namespace OneTox.ViewModel.ProfileSettings
{
    public class ProfileViewModel
    {
        private readonly ToxData _toxData;
        private readonly ToxDataInfo _toxDataInfo;

        private ProfileViewModel(ToxData toxData, ToxDataInfo toxDataInfo)
        {
            _toxData = toxData;
            _toxDataInfo = toxDataInfo;
        }

        public ToxId Id => _toxDataInfo.Id;
        public string Name => _toxDataInfo.Name;
        public ToxUserStatus Status => _toxDataInfo.Status;
        public string StatusMessage => _toxDataInfo.StatusMessage;

        public static async Task<ProfileViewModel> GetProfileViewModelFromFile(StorageFile file)
        {
            var data = (await FileIO.ReadBufferAsync(file)).ToArray();
            var toxData = ToxData.FromBytes(data);

            ToxDataInfo toxDataInfo;
            toxData.TryParse(out toxDataInfo);
            if (toxDataInfo == null)
                return null;

            return new ProfileViewModel(toxData, toxDataInfo);
        }

        public async Task DeleteBackingFile()
        {
            var file = await ApplicationData.Current.RoamingFolder.GetFileAsync(_toxDataInfo.Id.PublicKey + ".tox");
            await file.DeleteAsync();
        }

        public async Task SetAsCurrent()
        {
            var toxInstance = new ExtendedTox(new ToxOptions(true, true), _toxData);
            ToxModel.Instance.SetCurrent(toxInstance);
            await ToxModel.Instance.SaveDataAsync();
            ToxModel.Instance.Start();
            await AvatarManager.Instance.LoadAvatars();
        }
    }
}