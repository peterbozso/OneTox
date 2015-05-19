using System.Collections.ObjectModel;
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
            ToxModel.Instance.FriendMessageReceived += FriendMessageReceivedHandler;
            ToxModel.Instance.FriendTypingChanged += FriendTypingChangedHandler;
        }

        public ObservableCollection<FriendViewModel> Friends { get; set; }

        private void FriendNameChangedHandler(object sender, ToxEventArgs.NameChangeEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { FindFriend(e.FriendNumber).Name = e.Name; });
        }

        private void FriendStatusMessageChangedHandler(object sender, ToxEventArgs.StatusMessageEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { FindFriend(e.FriendNumber).StatusMessage = e.StatusMessage; });
        }

        private void FriendStatusChangedHandler(object sender, ToxEventArgs.StatusEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { FindFriend(e.FriendNumber).Status = e.Status; });
        }

        private void FriendConnectionStatusChangedHandler(object sender, ToxEventArgs.FriendConnectionStatusEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { FindFriend(e.FriendNumber).IsConnected = e.Status != ToxConnectionStatus.None; });
        }

        private void FriendListChangedHandler(object sender, FriendListChangedEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
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

        private void FriendMessageReceivedHandler(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            FindFriend(e.FriendNumber).ReceiveMessage(e);
        }

        private void FriendTypingChangedHandler(object sender, ToxEventArgs.TypingStatusEventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { FindFriend(e.FriendNumber).SetIsTyping(e.IsTyping); });
        }

        private FriendViewModel FindFriend(int friendNumber)
        {
            foreach (var friend in Friends)
            {
                if (friend.FriendNumber == friendNumber)
                    return friend;
            }
            return null;
        }
    }
}