using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel.Friends
{
    public class FriendListViewModel
    {
        // We need to run the event handlers from the UI thread.
        // Otherwise the PropertyChanged events wouldn't work in FriendViewModel.

        private readonly CoreDispatcher _dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

        public FriendListViewModel()
        {
            Friends = new ObservableCollection<FriendViewModel>();
            foreach (var friendNumber in ToxModel.Instance.Friends)
            {
                Friends.Add(new FriendViewModel(friendNumber));
            }

            ToxModel.Instance.FriendNameChanged += FriendNameChangedHandler;
            ToxModel.Instance.FriendStatusMessageChanged += FriendStatusMessageChangedHandler;
            ToxModel.Instance.FriendStatusChanged += FriendStatusChangedHandler;
            ToxModel.Instance.FriendConnectionStatusChanged += FriendConnectionStatusChangedHandler;
            ToxModel.Instance.FriendListChanged += FriendListChangedHandler;
        }

        public ObservableCollection<FriendViewModel> Friends { get; set; }

        private async void FriendNameChangedHandler(object sender, ToxEventArgs.NameChangeEventArgs e)
        {
            await
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { FindFriend(e.FriendNumber).Name = e.Name; });
        }

        private async void FriendStatusMessageChangedHandler(object sender, ToxEventArgs.StatusMessageEventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { FindFriend(e.FriendNumber).StatusMessage = e.StatusMessage; });
        }

        private async void FriendStatusChangedHandler(object sender, ToxEventArgs.StatusEventArgs e)
        {
            await
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () => { FindFriend(e.FriendNumber).Status = e.Status; });
        }

        private async void FriendConnectionStatusChangedHandler(object sender,
            ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { FindFriend(e.FriendNumber).IsConnected = e.Status != ToxConnectionStatus.None; });
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

        private FriendViewModel FindFriend(int friendNumber)
        {
            return Friends.FirstOrDefault(friend => friend.FriendNumber == friendNumber);
        }
    }
}