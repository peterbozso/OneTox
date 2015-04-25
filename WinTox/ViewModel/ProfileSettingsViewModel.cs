using System;
using System.Text;
using System.Threading.Tasks;
using SharpTox.Core;

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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        public ToxUserStatus Status
        {
            get { return App.ToxModel.Status; }
            set
            {
                App.ToxModel.Status = value;
                OnPropertyChanged();
            }
        }

        public async Task SaveDataAsync()
        {
            await App.ToxModel.SaveDataAsync();
        }

        public void ResetNospam()
        {
            var rand = new Random();
            var nospam = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                nospam[i] = (byte) rand.Next(10);
            }
            App.ToxModel.SetNospam(BitConverter.ToUInt32(nospam, 0));
            OnPropertyChanged("Id");
        }
    }
}