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
        public MainViewModel(IDataService dataService)
        {
            FriendList = new FriendListViewModel(dataService);
            FriendRequests = new FriendRequestsViewModel(dataService.ToxModel);
        }

        public FriendListViewModel FriendList { get; }
        public FriendRequestsViewModel FriendRequests { get; }
    }
}