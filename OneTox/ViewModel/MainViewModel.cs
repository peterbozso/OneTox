using System;
using System.Collections.Specialized;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Threading;
using OneTox.Config;
using OneTox.Helpers;
using OneTox.ViewModel.FriendRequests;
using OneTox.ViewModel.Friends;
using SharpTox.Core;

namespace OneTox.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private Visibility _friendRequestsListVisibility;

        public MainViewModel(IDataService dataService)
        {
            FriendList = new FriendListViewModel(dataService);

            FriendRequests = new FriendRequestsViewModel(dataService.ToxModel);
            FriendRequests.FriendRequestReceived += FriendRequestReceivedHandler;
            FriendRequests.Requests.CollectionChanged += FriendRequestsCollectionChangedHandler;
            DecideFriendRequestsListVisibility();
        }

        public FriendListViewModel FriendList { get; }
        public FriendRequestsViewModel FriendRequests { get; }

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

        private void DecideFriendRequestsListVisibility()
        {
            FriendRequestsListVisibility = FriendRequests.Requests.Count > 0
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
                msgDialog.Commands.Add(new UICommand("Accept", null, FriendRequestsViewModel.FriendRequestAnswer.Accept));
                msgDialog.Commands.Add(new UICommand("Decline", null,
                    FriendRequestsViewModel.FriendRequestAnswer.Decline));
                msgDialog.Commands.Add(new UICommand("Later", null, FriendRequestsViewModel.FriendRequestAnswer.Later));
                var answer = await msgDialog.ShowAsync();
                FriendRequests.HandleFriendRequestAnswer((FriendRequestsViewModel.FriendRequestAnswer) answer.Id, e);
            });
        }

        private void FriendRequestsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            DecideFriendRequestsListVisibility();
        }
    }
}