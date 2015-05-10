using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;

namespace WinTox.ViewModel
{
    internal class ProfileSettingsViewModel : ViewModelBase
    {
        public ProfileSettingsViewModel()
        {
            App.ToxModel.PropertyChanged += ToxModelPropertyChangedHandler;
        }

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

        private void ToxModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { RaisePropertyChanged(e.PropertyName); });
        }

        public async Task SaveDataAsync()
        {
            await App.ToxModel.SaveDataAsync();
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