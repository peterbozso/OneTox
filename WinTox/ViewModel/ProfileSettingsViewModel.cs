using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Provider;
using SharpTox.Core;
using SharpTox.Encryption;

namespace WinTox.ViewModel
{
    internal class ProfileSettingsViewModel : ViewModelBase
    {
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
    }
}