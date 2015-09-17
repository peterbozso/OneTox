using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using OneTox.Config;
using OneTox.Model.Tox;
using SharpTox.Core;

namespace OneTox.ViewModel.FriendRequests
{
    public class FriendRequestsViewModel : ViewModelBase
    {
        public enum FriendRequestAnswer
        {
            Accept,
            Decline,
            Later
        }

        private const string KFileName = "FriendRequests";
        private readonly IToxModel _toxModel;

        private Visibility _friendRequestsListVisibility;

        public FriendRequestsViewModel(IDataService dataService)
        {
            _toxModel = dataService.ToxModel;

            Requests = new ObservableCollection<OneFriendRequestViewModel>();
            Requests.CollectionChanged += FriendRequestsCollectionChangedHandler;
            _toxModel.FriendRequestReceived += FriendRequestReceivedHandler;

            RestoreData();

            DecideFriendRequestsListVisibility();
        }

        public Visibility FriendRequestsListVisibility
        {
            get { return _friendRequestsListVisibility; }
            set
            {
                if (value == _friendRequestsListVisibility)
                    return;
                _friendRequestsListVisibility = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<OneFriendRequestViewModel> Requests { get; }

        private void DecideFriendRequestsListVisibility()
        {
            FriendRequestsListVisibility = Requests.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            // TODO: Turn it into a toast notification.
            DispatcherHelper.CheckBeginInvokeOnUI(async () =>
            {
                var message = "From: " + e.PublicKey + "\n" + "Message: " + e.Message;
                var msgDialog = new MessageDialog(message, "Friend request received");
                msgDialog.Commands.Add(new UICommand("Accept", null, FriendRequestAnswer.Accept));
                msgDialog.Commands.Add(new UICommand("Decline", null,
                    FriendRequestAnswer.Decline));
                msgDialog.Commands.Add(new UICommand("Later", null, FriendRequestAnswer.Later));
                var answer = await msgDialog.ShowAsync();
                HandleFriendRequestAnswer((FriendRequestAnswer) answer.Id, e);
            });
        }

        private void HandleFriendRequestAnswer(FriendRequestAnswer answer, ToxEventArgs.FriendRequestEventArgs e)
        {
            switch (answer)
            {
                case FriendRequestAnswer.Accept:
                    _toxModel.AddFriendNoRequest(e.PublicKey);
                    return;

                case FriendRequestAnswer.Decline:
                    return;

                case FriendRequestAnswer.Later:
                    Requests.Add(new OneFriendRequestViewModel(_toxModel, this, e.PublicKey, e.Message));
                    return;
            }
        }

        private async void FriendRequestsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            DecideFriendRequestsListVisibility();
            await SaveDataAsync();
        }

        private async Task SaveDataAsync()
        {
            var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync(
                KFileName, CreationCollisionOption.ReplaceExisting);

            var requestStrings = new List<string>();
            foreach (var friendRequest in Requests)
            {
                var oneRequestString = new string[2];
                oneRequestString[0] = friendRequest.PublicKey;
                oneRequestString[1] = friendRequest.Message;
                requestStrings.AddRange(oneRequestString);
            }

            await FileIO.WriteLinesAsync(file, requestStrings);
        }

        private async Task RestoreData()
        {
            try
            {
                var file = await ApplicationData.Current.RoamingFolder.GetFileAsync(KFileName);

                var lines = await FileIO.ReadLinesAsync(file);

                for (var i = 0; i < lines.Count; i += 2)
                {
                    var publicKey = lines[i];
                    var message = lines[i + 1];
                    Requests.Add(new OneFriendRequestViewModel(_toxModel, this, new ToxKey(ToxKeyType.Public, publicKey),
                        message));
                }
            }
            catch (FileNotFoundException)
            {
                // The file was not there, so we cannot restore state, but it's not a problem:
                // it means we haven't saved any requests previously. So just keep going as if nothing had happened!
            }
        }
    }
}