using OneTox.Model;
using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace OneTox.ViewModel.FriendRequests
{
    public class FriendRequestsViewModel
    {
        public enum FriendRequestAnswer
        {
            Accept,
            Decline,
            Later
        }

        private const string KFileName = "FriendRequests";
        private readonly SemaphoreSlim _semaphore;

        public FriendRequestsViewModel()
        {
            Requests = new ObservableCollection<OneFriendRequestViewModel>();
            Requests.CollectionChanged += FriendRequestsCollectionChangedHandler;
            ToxModel.Instance.FriendRequestReceived += FriendRequestReceivedHandler;
            _semaphore = new SemaphoreSlim(1);
        }

        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;

        public ObservableCollection<OneFriendRequestViewModel> Requests { get; }

        public void HandleFriendRequestAnswer(FriendRequestAnswer answer, ToxEventArgs.FriendRequestEventArgs e)
        {
            switch (answer)
            {
                case FriendRequestAnswer.Accept:
                    ToxModel.Instance.AddFriendNoRequest(e.PublicKey);
                    return;

                case FriendRequestAnswer.Decline:
                    return;

                case FriendRequestAnswer.Later:
                    Requests.Add(new OneFriendRequestViewModel(this, e.PublicKey, e.Message));
                    return;
            }
        }

        public async Task RestoreData()
        {
            try
            {
                var file = await ApplicationData.Current.RoamingFolder.GetFileAsync(KFileName);

                var lines = await FileIO.ReadLinesAsync(file);

                for (var i = 0; i < lines.Count; i += 2)
                {
                    var publicKey = lines[i];
                    var message = lines[i + 1];
                    Requests.Add(new OneFriendRequestViewModel(this, new ToxKey(ToxKeyType.Public, publicKey), message));
                }
            }
            catch (FileNotFoundException)
            {
                // The file was not there, so we cannot restore state, but no problem: keep going as if nothing had happened!
            }
        }

        public async Task SaveDataAsync()
        {
            await _semaphore.WaitAsync();
            try
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
            finally
            {
                _semaphore.Release();
            }
        }

        private void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            FriendRequestReceived?.Invoke(sender, e);
        }

        private async void FriendRequestsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            await SaveDataAsync();
        }
    }
}
