using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using OneTox.Config;
using OneTox.Model;

namespace OneTox.ViewModel.Friends
{
    public class FriendListViewModel
    {
        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        private readonly IDataService _dataService;
        private readonly IToxModel _toxModel;

        public FriendListViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _toxModel = dataService.ToxModel;

            Friends = new ObservableCollection<FriendViewModel>();
            foreach (var friendNumber in _toxModel.Friends)
            {
                Friends.Add(new FriendViewModel(dataService, friendNumber));
            }

            _toxModel.FriendListChanged += FriendListChangedHandler;
        }

        public ObservableCollection<FriendViewModel> Friends { get; set; }

        private FriendViewModel FindFriend(int friendNumber)
        {
            return Friends.FirstOrDefault(friend => friend.FriendNumber == friendNumber);
        }

        private async void FriendListChangedHandler(object sender, FriendListChangedEventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    switch (e.Action)
                    {
                        case FriendListChangedAction.Add:
                            Friends.Add(new FriendViewModel(_dataService, e.FriendNumber));
                            return;

                        case FriendListChangedAction.Remove:
                            Friends.Remove(FindFriend(e.FriendNumber));
                            return;

                        case FriendListChangedAction.Reset:
                            Friends.Clear();
                            foreach (var friendNumber in _toxModel.Friends)
                            {
                                Friends.Add(new FriendViewModel(_dataService, friendNumber));
                            }
                            return;
                    }
                });
        }
    }
}