using SharpTox.Core;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;

namespace WinTox.ViewModel
{
    internal class MainPageViewModel
    {
        internal MainPageViewModel()
        {
            FriendList = new FriendListViewModel();
            App.ToxModel.FriendRequestReceived += FriendRequestReceivedHandler;
        }

        public FriendListViewModel FriendList { get; set; }

        internal enum FriendRequestAnswer
        {
            Accept,
            Decline,
            Later
        }

        internal void HandleFriendRequestAnswer(FriendRequestAnswer answer, ToxEventArgs.FriendRequestEventArgs e)
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
