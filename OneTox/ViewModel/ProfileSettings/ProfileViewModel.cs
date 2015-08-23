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
        private readonly ExtendedTox _toxInstance;

        private ProfileViewModel(ExtendedTox toxInstance)
        {
            _toxInstance = toxInstance;
        }

        public string Name => _toxInstance.Name;

        public static async Task<ProfileViewModel> GetProfileViewModelFromFile(StorageFile file)
        {
            var data = (await FileIO.ReadBufferAsync(file)).ToArray();
            return new ProfileViewModel(new ExtendedTox(new ToxOptions(true, true), ToxData.FromBytes(data)));
        }

        public static ProfileViewModel GetDefaultProfileViewModel()
        {
            var toxInstance = new ExtendedTox(new ToxOptions(true, true))
            {
                Name = "User",
                StatusMessage = "Using OneTox."
            };

            return new ProfileViewModel(toxInstance);
        }

        public async Task SetAsCurrent()
        {
            ToxModel.Instance.SetCurrent(_toxInstance);
            await ToxModel.Instance.SaveDataAsync();
            ToxModel.Instance.Start();
            await AvatarManager.Instance.LoadAvatars();
        }
    }
}