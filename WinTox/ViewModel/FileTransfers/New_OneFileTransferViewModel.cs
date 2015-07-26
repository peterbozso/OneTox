using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using WinTox.Model;

namespace WinTox.ViewModel.FileTransfers
{
    public class New_OneFileTransferViewModel : ViewModelBase
    {
        private readonly OneFileTransferModel _transferModel;
        private double _progress;

        public New_OneFileTransferViewModel(OneFileTransferModel fileTransferModel)
        {
            _transferModel = fileTransferModel;
            _transferModel.PropertyChanged += ModelPropertyChangedHandler;
        }

        public string Name
        {
            get { return _transferModel.Name; }
        }

        public TransferDirection Direction
        {
            get { return _transferModel.Direction; }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (value.Equals(_progress))
                    return;
                _progress = value;
                RaisePropertyChanged();
            }
        }

        public FileTransferState State
        {
            get { return _transferModel.State; }
        }

        private async void ModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            await
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { RaisePropertyChanged(e.PropertyName); });
        }
    }
}