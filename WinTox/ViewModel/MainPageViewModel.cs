using System;
using SharpTox.Core;
using WinTox.ViewModel.Friends;

namespace WinTox.ViewModel
{
    public class MainPageViewModel
    {
        public enum FriendRequestAnswer
        {
            Accept,
            Decline,
            Later
        }

        public MainPageViewModel()
        {
            FriendList = new FriendListViewModel();
            App.ToxModel.FriendRequestReceived += FriendRequestReceivedHandler;
        }

        public FriendListViewModel FriendList { get; set; }

        public void HandleFriendRequestAnswer(FriendRequestAnswer answer, ToxEventArgs.FriendRequestEventArgs e)
        {
            switch (answer)
            {
                case FriendRequestAnswer.Accept:
                    ToxErrorFriendAdd error;
                    App.ToxModel.AddFriendNoRequest(e.PublicKey, out error);
                    // TODO: Handle error!
                    return;

                case FriendRequestAnswer.Decline:
                    // TODO: ?
                    return;

                case FriendRequestAnswer.Later:
                    // TODO: Postpone decision!
                    return;
            }
        }

        public event EventHandler<ToxEventArgs.FriendRequestEventArgs> FriendRequestReceived;

        private void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            if (FriendRequestReceived != null)
                FriendRequestReceived(sender, e);
        }
    }
}