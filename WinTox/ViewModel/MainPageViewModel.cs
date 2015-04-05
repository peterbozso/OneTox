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

        private enum FriendRequestAnswer
        {
            Accept,
            Decline,
            Later
        }

        private void FriendRequestReceivedHandler(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var msgDialog = new MessageDialog(e.Message, e.PublicKey.ToString().Substring(0, 10));
                msgDialog.Commands.Add(new UICommand("Accept", null, FriendRequestAnswer.Accept));
                msgDialog.Commands.Add(new UICommand("Decline", null, FriendRequestAnswer.Decline));
                msgDialog.Commands.Add(new UICommand("Later", null, FriendRequestAnswer.Later));
                var answer = await msgDialog.ShowAsync();
                HandleFriendRequestAnswer((FriendRequestAnswer)answer.Id, e);
            });
        }

        private void HandleFriendRequestAnswer(FriendRequestAnswer answer, ToxEventArgs.FriendRequestEventArgs e)
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
