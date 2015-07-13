using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel.FriendRequests
{
    /// <summary>
    ///     Implements the Singleton pattern. (https://msdn.microsoft.com/en-us/library/ff650849.aspx)
    /// </summary>
    public class FriendRequestsViewModel
    {
        public enum FriendRequestAnswer
        {
            Accept,
            Decline,
            Later
        }

        private const string KFileName = "FriendRequests";
        private static FriendRequestsViewModel _instance;
        private readonly SemaphoreSlim _semaphore;

        private FriendRequestsViewModel()
        {
            FriendRequests = new ObservableCollection<OneFriendRequestViewModel>();
            FriendRequests.CollectionChanged += FriendRequestsCollectionChangedHandler;
            ToxModel.Instance.FriendRequestReceived += FriendRequestReceivedHandler;
            _semaphore = new SemaphoreSlim(1);
        }

        public static FriendRequestsViewModel Instance
        {
            get { return _instance ?? (_instance = new FriendRequestsViewModel()); }
        }

        public ObservableCollection<OneFriendRequestViewModel> FriendRequests { get; private set; }
        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;

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
                    FriendRequests.Add(new OneFriendRequestViewModel(e.PublicKey, e.Message));
                    return;
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
                foreach (var friendRequest in FriendRequests)
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

        public async Task RestoreDataAsync()
        {
            try
            {
                var file = await ApplicationData.Current.RoamingFolder.GetFileAsync(KFileName);

                var lines = await FileIO.ReadLinesAsync(file);

                for (var i = 0; i < lines.Count; i += 2)
                {
                    var publicKey = lines[i];
                    var message = lines[i + 1];
                    FriendRequests.Add(new OneFriendRequestViewModel(new ToxKey(ToxKeyType.Public, publicKey), message));
                }
            }
            catch (FileNotFoundException)
            {
                // The file was not there, so we cannot restore state, but no problem: keep going as if nothing had happened!
            }
        }

        private async void FriendRequestsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            await SaveDataAsync();
        }

        private void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            if (FriendRequestReceived != null)
                FriendRequestReceived(sender, e);
        }
    }
}