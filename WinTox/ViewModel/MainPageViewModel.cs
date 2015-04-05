using SharpTox.Core;

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

        internal delegate void FriendRequestReceivedEventHandler(ToxEventArgs.FriendRequestEventArgs e);

        internal event FriendRequestReceivedEventHandler FriendRequestReceived;

        internal void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            if (FriendRequestReceived != null)
            {
                FriendRequestReceived(e);
            }
        }

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
    }
}
