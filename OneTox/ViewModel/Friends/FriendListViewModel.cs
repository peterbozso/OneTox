using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using OneTox.Model;

namespace OneTox.ViewModel.Friends
{
    public class FriendListViewModel
    {
        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

        public FriendListViewModel()
        {
            Friends = new ObservableCollection<FriendViewModel>();
            foreach (var friendNumber in ToxModel.Instance.Friends)
            {
                Friends.Add(new FriendViewModel(friendNumber));
            }

            ToxModel.Instance.FriendListChanged += FriendListChangedHandler;
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
                            Friends.Add(new FriendViewModel(e.FriendNumber));
                            return;

                        case FriendListChangedAction.Remove:
                            Friends.Remove(FindFriend(e.FriendNumber));
                            return;

                        case FriendListChangedAction.Reset:
                            Friends.Clear();
                            foreach (var friendN in ToxModel.Instance.Friends)
                            {
                                Friends.Add(new FriendViewModel(friendN));
                            }
                            return;
                    }
                });
        }
    }
}